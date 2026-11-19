
<script setup lang="ts">
import { ref } from 'vue'
import AppSidebar from '../components/AppSidebar.vue'
import AppHeader from '../components/AppHeader.vue'

const sidebarOpen = ref(false)
</script>

<template>
    <div class="app-layout">
        <AppSidebar />

        <div
            v-if="sidebarOpen"
            class="sidebar-overlay"
            @click="sidebarOpen = false"
        ></div>

        <aside class="mobile-sidebar" :class="{ open: sidebarOpen }">
            <AppSidebar />
        </aside>

        <main class="main-content">
            <AppHeader :on-menu-click="() => sidebarOpen = !sidebarOpen" />

            <section class="page-content">
                <RouterView />
            </section>
        </main>
    </div>
</template>
