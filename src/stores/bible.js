import { defineStore } from "pinia";
import ntv from "../assets/json/NTV.json";

const bibleContent = ntv?.bible.b;
const rawVersions = ["RVR1960", "NTV"];
const rawBooks = [];
const version = "NTV";
let bookName = "";
let chapterNumber = null;
let verseNumber = null;

export const useBibleStore = defineStore({
  id: "bible",
  state: () => ({
    version,
    rawVersions,
    bookName,
    rawBooks,
    chapterNumber,
    verseNumber,
  }),
  getters: {
    versions: (state) => state.rawVersions,
    books: (state) => {
      if (!state.rawBooks || state.rawBooks != {}) {
        state.rawBooks = bibleContent.map((book) => book._n);
      }
      return state.rawBooks;
    },
  },
  actions: {
    getChapterNumbers() {
      chapterNumber = null;
      let bookToSearch = bibleContent.find((book) => book._n === bookName);
      console.log(bookName);
      if (bookToSearch) {
        chapterNumber = bookToSearch?.c.length;
      }
      return chapterNumber;
    },
    getVerseNumbers() {
      verseNumber = null;
      let bookToSearch = bibleContent.find((book) => book._n === bookName);
      if (bookToSearch && chapterNumber) {
        verseNumber = bookToSearch?.c[chapterNumber - 1]?.v.length;
      }
      return verseNumber;
    },
  },
});
