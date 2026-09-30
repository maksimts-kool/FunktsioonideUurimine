import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Arenduses suunatakse /api päringud ASP.NET Core serverisse; `npm run build` kirjutab
// valmis rakenduse backendi wwwroot kausta, kust `dotnet run` seda serveerib.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5176',
    },
  },
  build: {
    outDir: '../backend/FunktsioonideUurimine.Api/wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 700, // ECharts (juba tree-shaken) on ~600 kB
    rolldownOptions: {
      output: {
        // suured teegid eraldi failidesse – brauser saab neid eraldi vahemällu hoida
        codeSplitting: {
          groups: [
            { name: 'echarts', test: /node_modules[\\/](echarts|zrender)[\\/]/ },
            { name: 'katex', test: /node_modules[\\/]katex[\\/]/ },
            { name: 'mantine', test: /node_modules[\\/]@mantine[\\/]/ },
            { name: 'react', test: /node_modules[\\/](react|react-dom|react-router|scheduler|@tanstack)[\\/]/ },
          ],
        },
      },
    },
  },
})
