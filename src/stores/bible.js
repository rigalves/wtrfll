import { defineStore } from "pinia";
import ntv from "../assets/json/NTV.json";
import rvr from "../assets/json/RVR1960.json";

const bibleData = {
  NTV: ntv?.bible.b,
  RVR1960: rvr?.bible.b,
};

const rawVersions = ["RVR1960", "NTV"];

// reactive state defaults
const version = "NTV";
let bookName = "";
let chapterNumber = null;
let verseNumber = null;
let verseText = "";

export const useBibleStore = defineStore({
  id: "bible",
  state: () => ({
    version,
    rawVersions,
    bookName,
    chapterNumber,
    verseNumber,
    verseText,
  }),
  getters: {
    versions: (state) => state.rawVersions,
    books: (state) => {
      return bibleData[state.version].map((book) => book._n);
    },
  },
  actions: {
    getChapterNumbers() {
      this.chapterNumber = null;
      const bibleContent = bibleData[this.version];
      const bookToSearch = bibleContent.find((book) => book._n === this.bookName);
      if (bookToSearch) {
        this.chapterNumber = bookToSearch.c.length;
      }
      return this.chapterNumber;
    },
    getVerseNumbers() {
      this.verseNumber = null;
      const bibleContent = bibleData[this.version];
      const bookToSearch = bibleContent.find((book) => book._n === this.bookName);
      if (bookToSearch && this.chapterNumber) {
        this.verseNumber = bookToSearch.c[this.chapterNumber - 1]?.v.length;
      }
      return this.verseNumber;
    },
    updateVerseText() {
      const bibleContent = bibleData[this.version];
      const book = bibleContent.find((b) => b._n === this.bookName);
      let text = "";
      if (book && this.chapterNumber && this.verseNumber) {
        const verse = book.c[this.chapterNumber - 1]?.v.find(
          (v) => v._n === String(this.verseNumber)
        );
        text = verse ? verse.__text : "";
      }
      this.verseText = text;
    },
  },
});
