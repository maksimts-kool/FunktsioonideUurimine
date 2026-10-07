import '@mantine/core/styles.css'
import '@mantine/notifications/styles.css'
import 'katex/dist/katex.min.css'

import { createTheme, MantineProvider } from '@mantine/core'
import { ModalsProvider } from '@mantine/modals'
import { Notifications } from '@mantine/notifications'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { ApiViga } from './api/klient'
import { Avaleht } from './lehed/Avaleht'
import { Detailvaade } from './lehed/Detailvaade'
import { EiLeitud } from './lehed/EiLeitud'
import { Paigutus } from './lehed/Paigutus'
import { Salvestatud } from './lehed/Salvestatud'
import { Teooria } from './lehed/Teooria'
import { UusFunktsioon } from './lehed/Vormilehed'

const teema = createTheme({
  primaryColor: 'indigo',
  defaultRadius: 'md',
})

const paringuKlient = new QueryClient({
  defaultOptions: {
    queries: {
      // valideerimis- ja 404-vigu pole mõtet korrata
      retry: (korduseid, viga) => !(viga instanceof ApiViga && viga.status < 500) && korduseid < 2,
      refetchOnWindowFocus: false,
    },
  },
})

const marsruudid = createBrowserRouter([
  {
    path: '/',
    element: <Paigutus />,
    children: [
      { index: true, element: <Avaleht /> },
      { path: 'uus', element: <UusFunktsioon /> },
      { path: 'teooria', element: <Teooria /> },
      { path: 'funktsioonid', element: <Salvestatud /> },
      { path: 'funktsioon/:id', element: <Detailvaade /> },
      { path: '*', element: <EiLeitud /> },
    ],
  },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider theme={teema} defaultColorScheme="auto">
      <QueryClientProvider client={paringuKlient}>
        <ModalsProvider>
          <Notifications position="top-right" />
          <RouterProvider router={marsruudid} />
        </ModalsProvider>
      </QueryClientProvider>
    </MantineProvider>
  </StrictMode>,
)
