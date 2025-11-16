import { createRouter, createWebHistory } from 'vue-router'

const HomeView = () => import('@/views/HomeView.vue')
const ControllerView = () => import('@/views/ControllerView.vue')
const DisplayView = () => import('@/views/DisplayView.vue')
const NewSessionView = () => import('@/views/NewSessionView.vue')

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: HomeView },
    { path: '/new', name: 'new-session', component: NewSessionView },
    { path: '/control/:sessionId', name: 'control', component: ControllerView, props: true },
    { path: '/display/:sessionId', name: 'display', component: DisplayView, props: true },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})
