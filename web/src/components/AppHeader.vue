
<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { Bell, Menu } from 'lucide-vue-next'

defineProps<{
    onMenuClick?: () => void
}>()

const route = useRoute()

const pageTitle = computed(() => {
    return route.meta.title || 'Dashboard'
})
</script>

<template>
    <header class="header">
        <div class="header-left">
            <button
                class="mobile-menu"
                @click="onMenuClick"
                aria-label="Open navigation"
            >
                <Menu :size="21" />
            </button>

            <div>
                <h1>{{ pageTitle }}</h1>
                <p>Manage your operations and activities.</p>
            </div>
        </div>

        <div class="header-right">
            <button class="icon-button" aria-label="Notifications">
                <Bell :size="20" />
            </button>

            <div class="header-divider"></div>

            <div class="user-info">
                <div class="user-avatar">AD</div>

                <div class="user-details">
                    <strong>Administrator</strong>
                    <span>Admin</span>
                </div>
            </div>
        </div>
    </header>
</template>
