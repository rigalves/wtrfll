<script>
import { ref } from 'vue'
//import { mapWritableState } from 'pinia'
import { useBibleStore } from '../../stores/bible'

export default {
    /*computed: {
        ...mapWritableState(useBibleStore, ['versions'], ['books'])
    },*/
    setup() {
        const bible = useBibleStore()
        const versions = ref(bible.versions)
        const books = ref(bible.books)
        const chapters = ref(null)
        const verses = ref(null)

        return {
            bible,
            versions,
            books,
            chapters,
            verses,
            filterVersions(val, update) {
                update(() => {
                    const needle = val.toLocaleLowerCase()
                    versions.value = bible.versions.filter(v => v.toLocaleLowerCase().indexOf(needle) > -1)
                })
            },
            filterBooks(val, update) {
                update(() => {
                    const needle = val.toLocaleLowerCase()
                    books.value = bible.books.filter(v => v.toLocaleLowerCase().indexOf(needle) > -1)
                    chapters.value = [...Array(bible.getChapterNumbers()).keys()].map(x => x + 1)
                    console.log(chapters.value)
                })
            },
            filterVerses() {
                verses.value = [...Array(bible.getVerseNumbers()).keys()].map(x => x + 1)
            }
        }
    }
}
</script>
    
<template>
    <q-form @submit="onSubmit" class="q-gutter-md">
        <q-select filled v-model="bible.version" :options="versions" label="Versión" hint="Ej. RVR1960, NTV" use-input
            hide-selected fill-input input-debounce="0" @filter="filterVersions" lazy-rules 
            :rules="[ val => val && val.length > 0 || 'Favor digitar la versión']">
            <template v-slot:no-option>
                <q-item>
                    <q-item-section class="text-grey">
                        Sin resultados
                    </q-item-section>
                </q-item>
            </template>
        </q-select>

        <q-select filled v-model="bible.bookName" :options="books" label="Libro" hint="Ej. Génesis, Éxodo" use-input
            hide-selected fill-input input-debounce="0" @filter="filterBooks" lazy-rules
            :rules="[ val => val && val.length > 0 || 'Favor digitar el libro']">
            <template v-slot:no-option>
                <q-item>
                    <q-item-section class="text-grey">
                        Sin resultados
                    </q-item-section>
                </q-item>
            </template>
        </q-select>

        <q-select filled v-model="bible.chapterNumber" :options="chapters" label="Capítulo" use-input hide-selected
            fill-input input-debounce="0" @input-value="filterVerses">
            <template v-slot:no-option>
                <q-item>
                    <q-item-section class="text-grey">
                        Sin resultados
                    </q-item-section>
                </q-item>
            </template>
        </q-select>

        <q-select filled v-model="bible.verseNumber" :options="verses" label="Versículo" use-input hide-selected
            fill-input input-debounce="0">
            <template v-slot:no-option>
                <q-item>
                    <q-item-section class="text-grey">
                        Sin resultados
                    </q-item-section>
                </q-item>
            </template>
        </q-select>

        <div>
            <q-btn label="Guardar" type="submit" color="primary" />
        </div>
    </q-form>
</template>