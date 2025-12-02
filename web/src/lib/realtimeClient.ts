import * as signalR from '@microsoft/signalr'

const REALTIME_URL = import.meta.env.VITE_REALTIME_URL ?? '/realtime'
const ENABLE_REALTIME = (import.meta.env.VITE_ENABLE_REALTIME ?? 'true') === 'true'

export type DisplayCommand = 'normal' | 'black' | 'clear' | 'freeze'

export interface SessionPresentationOptions {
  showVerseNumbers?: boolean
  showReference?: boolean
  fontScale?: number
  safeMarginPct?: number
  theme?: string
}

export interface SessionStatePatchPayload {
  contractVersion: number
  sessionId: string
  patch: {
    translation: string
    passageRef: string
    currentIndex?: number
    options?: SessionPresentationOptions
    displayCommand?: DisplayCommand
  }
}

export interface LyricsStatePatchPayload {
  contractVersion: number
  sessionId: string
  patch: {
    lyricsId?: string | null
    title?: string | null
    author?: string | null
    lyricsChordPro: string
    fontScale?: number
    columnCount?: number
  }
}

export interface SessionStateUpdatePayload {
  contractVersion: number
  sessionId: string
  state: {
    translation: string
    reference: string
    verses: Array<{
      book: string
      chapter: number
      verse: number
      text: string
    }>
    currentIndex: number
    options?: SessionPresentationOptions
    attribution?: {
      required: boolean
      text?: string | null
      url?: string | null
    }
    displayCommand?: DisplayCommand
  }
}

export interface LyricsStateUpdatePayload {
  contractVersion: number
  sessionId: string
  state: {
    lyricsId?: string | null
    title?: string | null
    author?: string | null
    lines: string[]
    fontScale?: number
    columnCount?: number
  }
}

export interface SessionHeartbeatResponse {
  contractVersion: number
  sessionId: string
  role: string
  serverTimestamp: string
}

type SessionRealtimeClientOptions = {
  sessionId: string
  joinToken: string
  role: 'controller' | 'display'
}

export class SessionRealtimeClient {
  private connection: signalR.HubConnection | null = null
  private readonly options: SessionRealtimeClientOptions
  private readonly stateUpdateHandlers = new Set<(payload: SessionStateUpdatePayload) => void>()
  private readonly lyricsUpdateHandlers = new Set<(payload: LyricsStateUpdatePayload) => void>()
  private readonly heartbeatHandlers = new Set<(payload: SessionHeartbeatResponse) => void>()
  private connectPromise: Promise<void> | null = null
  private readonly handleStateUpdate = (payload: SessionStateUpdatePayload) => {
    this.stateUpdateHandlers.forEach((handler) => handler(payload))
  }
  private readonly handleLyricsUpdate = (payload: LyricsStateUpdatePayload) => {
    this.lyricsUpdateHandlers.forEach((handler) => handler(payload))
  }
  private readonly handleHeartbeat = (payload: SessionHeartbeatResponse) => {
    this.heartbeatHandlers.forEach((handler) => handler(payload))
  }

  constructor(options: SessionRealtimeClientOptions) {
    this.options = options
  }

  async connect(): Promise<void> {
    if (!ENABLE_REALTIME) {
      return
    }
    if (this.connection) {
      return
    }
    if (this.connectPromise) {
      return this.connectPromise
    }

    this.connectPromise = this.startConnection()
    return this.connectPromise
  }

  private async startConnection(): Promise<void> {
    const hubUrl = new URL(REALTIME_URL, window.location.origin)
    hubUrl.searchParams.set('sessionId', this.options.sessionId)
    hubUrl.searchParams.set('role', this.options.role)
    hubUrl.searchParams.set('joinToken', this.options.joinToken)

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl.toString(), {
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .build()

    connection.on('state:update', this.handleStateUpdate)
    connection.on('lyrics:update', this.handleLyricsUpdate)
    connection.on('heartbeat', this.handleHeartbeat)

    try {
      await connection.start()
      this.connection = connection
    } catch (error) {
      connection.off('state:update', this.handleStateUpdate)
      connection.off('lyrics:update', this.handleLyricsUpdate)
      connection.off('heartbeat', this.handleHeartbeat)
      this.connectPromise = null
      throw error
    }
  }

  onStateUpdate(handler: (payload: SessionStateUpdatePayload) => void): void {
    this.stateUpdateHandlers.add(handler)
  }

  onLyricsUpdate(handler: (payload: LyricsStateUpdatePayload) => void): void {
    this.lyricsUpdateHandlers.add(handler)
  }

  onHeartbeat(handler: (payload: SessionHeartbeatResponse) => void): void {
    this.heartbeatHandlers.add(handler)
  }

  async sendPatch(payload: SessionStatePatchPayload): Promise<void> {
    if (!ENABLE_REALTIME) {
      return
    }

    await this.connect()
    if (!this.connection) {
      return
    }

    await this.connection.invoke('StatePatch', payload)
  }

  async sendLyricsPatch(payload: LyricsStatePatchPayload): Promise<void> {
    if (!ENABLE_REALTIME) {
      return
    }
    await this.connect()
    if (!this.connection) {
      return
    }
    await this.connection.invoke('PublishLyrics', payload)
  }

  async sendHeartbeat(): Promise<void> {
    if (!ENABLE_REALTIME) {
      return
    }

    await this.connect()
    if (!this.connection) {
      return
    }

    await this.connection.invoke('Heartbeat', {
      contractVersion: 1,
      sessionId: this.options.sessionId,
    })
  }

  dispose(): void {
    if (this.connection) {
      this.connection.off('state:update', this.handleStateUpdate)
      this.connection.off('heartbeat', this.handleHeartbeat)
      this.connection.stop().catch(() => {})
      this.connection = null
    }
    this.connectPromise = null
    this.stateUpdateHandlers.clear()
    this.lyricsUpdateHandlers.clear()
    this.heartbeatHandlers.clear()
  }
}
