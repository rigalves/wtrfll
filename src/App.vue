<template>
  <q-layout view="hHh lpR lFf">
    <q-header elevated class="shadow-2 darkgrey">
      <q-toolbar>
        <q-btn round dense flat :ripple="false" size="19px" color="white" class="q-mr-sm" no-caps />
        <q-toolbar-title>
          WTRFLL (beta)
        </q-toolbar-title>
        <q-btn dense flat no-wrap>
          <q-avatar rounded size="20px">
            <img src="https://cdn.quasar.dev/img/avatar3.jpg">
          </q-avatar>
          <q-icon name="arrow_drop_down" size="16px" />

          <q-menu auto-close>
            <q-list dense>
              <!--q-item>
                <q-item-section>
                  <div>Signed in as <strong>Mary</strong></div>
                </q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable>
                <q-item-section>
                  <div>
                    <q-icon name="tag_faces" color="blue-9" size="18px" />
                    Set your status
                  </div>
                </q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable>
                <q-item-section>Your profile</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Your repositories</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Your projects</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Your stars</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Your gists</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable>
                <q-item-section>Help</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Settings</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Sign out</q-item-section>
              </q-item-->
            </q-list>
          </q-menu>
        </q-btn>
      </q-toolbar>
    </q-header>
    <q-drawer v-model="drawer" show-if-above :mini="miniState" @mouseover="miniState = false"
      @mouseout="miniState = true" mini-to-overlay :width="200" :breakpoint="500" bordered class="bg-grey-3 shadow-2">
      <q-scroll-area class="fit">
        <q-list padding>
          <q-item :active="selectedCardType === 'Biblia'" clickable v-ripple @click="selectedCardType = 'Biblia'">
            <q-item-section avatar>
              <q-icon name="book" />
            </q-item-section>

            <q-item-section>
              Biblia
            </q-item-section>
          </q-item>

          <q-item :active="selectedCardType === 'Letras'" clickable v-ripple @click="selectedCardType = 'Letras'">
            <q-item-section avatar>
              <q-icon name="lyrics" />
            </q-item-section>

            <q-item-section>
              Letras
            </q-item-section>
          </q-item>

          <q-item :active="selectedCardType === 'Imágenes'" clickable v-ripple @click="selectedCardType = 'Imágenes'">
            <q-item-section avatar>
              <q-icon name="image" />
            </q-item-section>

            <q-item-section>
              Imágenes
            </q-item-section>
          </q-item>

          <q-separator />

          <q-item clickable v-ripple>
            <q-item-section avatar>
              <q-icon name="settings" />
            </q-item-section>

            <q-item-section>
              Ajustes
            </q-item-section>
          </q-item>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <div class="row">
        <div class="col-sm-12 col-md-6" style="padding:10px;">
          <q-card class="my-card">
            <q-card-section>
              <div class="text-h6">Ajustes para {{ selectedCardType }}</div>
            </q-card-section>

            <q-card-section class="q-pt-none">
              <BibleSettings/>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-sm-12 col-md-6" style="padding:10px;">
          <q-card class="my-card text-white" style="background: radial-gradient(circle, #35a2ff 0%, #014a88 100%)">
            <q-card-section>
              <div class="text-h6">Prevista</div>
            </q-card-section>

            <q-card-section class="q-pt-none">
              {{ 2 }}
            </q-card-section>
          </q-card>
        </div>
      </div>
    </q-page-container>

    <q-footer elevated class="bg-grey-8 text-white">
      <q-scroll-area style="height: 150px;">
        <div class="row no-wrap" v-for="r in 4" :key="'r' + r">
          <div v-for="n in 20" :key="n" style="width: 150px" class="q-pa-sm">
            Lorem ipsum dolor sit amet consectetur adipisicing elit. Architecto fuga quae veritatis blanditiis sequi id
            expedita amet esse aspernatur! Iure, doloribus!
          </div>
        </div>
      </q-scroll-area>
    </q-footer>
  </q-layout>
</template>

<style>
.card-type-section .card-type-label {
  font-size: 12px;
  line-height: 24px;
  letter-spacing: .01785714em;
  font-weight: 500;
}
</style>

<script>
import BibleSettings from './components/bible/BibleSettings.vue'
import { ref } from 'vue'

export default {
  components: {
    BibleSettings
  },

  setup() {
    /*const $q = useQuasar()

    // calling here; equivalent to when component is created
    $q.dark.set(true)*/
    return {
      drawer: ref(false),
      miniState: ref(true),
      selectedCardType: "Biblia"
    }

  }
}
</script>