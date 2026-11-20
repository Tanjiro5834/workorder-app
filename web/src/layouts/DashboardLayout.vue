<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from '../components/AppSidebar.vue'
import AppHeader from '../components/AppHeader.vue'

const route = useRoute()
const sidebarOpen = ref(false)

watch(() => route.fullPath, () => {
    sidebarOpen.value = false
})

function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape' && sidebarOpen.value) {
        sidebarOpen.value = false
    }
}

onMounted(() => {
    window.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
    window.removeEventListener('keydown', handleKeydown)
})
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