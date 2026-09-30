import { notifications } from '@mantine/notifications'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from './klient'
import type { FunktsiooniPaering, GraafikuPaering } from './tyybid'

export const votmed = {
  koik: ['funktsioonid'] as const,
  yks: (id: number) => ['funktsioonid', id] as const,
  analuus: (p: FunktsiooniPaering) => ['analuus', p] as const,
  graafik: (p: GraafikuPaering) => ['graafik', p] as const,
}

export const useFunktsioonid = () => useQuery({ queryKey: votmed.koik, queryFn: api.funktsioonid })

export const useFunktsioon = (id: number, lubatud = true) =>
  useQuery({ queryKey: votmed.yks(id), queryFn: () => api.funktsioon(id), enabled: lubatud && Number.isFinite(id) })

/** Graafiku punktid arvutab server (MathNet.Symbolics); eelmine graafik jääb laadimise ajaks nähtavale. */
export const useGraafik = (p: GraafikuPaering | null) =>
  useQuery({
    queryKey: votmed.graafik(p ?? { valem: '', algus: 0, lopp: 0, samm: 0 }),
    queryFn: ({ signal }) => api.graafik(p!, signal),
    enabled: p !== null && p.valem.trim() !== '',
    placeholderData: keepPreviousData,
    staleTime: Infinity,
  })

export const useAnaluus = (p: FunktsiooniPaering | null) =>
  useQuery({
    queryKey: votmed.analuus(p ?? { valem: '', vahemikAlgus: 0, vahemikLopp: 0 }),
    queryFn: ({ signal }) => api.analuus(p!, signal),
    enabled: p !== null && p.valem.trim() !== '',
    placeholderData: keepPreviousData,
    staleTime: Infinity,
  })

export function useLoo() {
  const klient = useQueryClient()
  return useMutation({
    mutationFn: api.loo,
    onSuccess: (f) => {
      klient.setQueryData(votmed.yks(f.id), f)
      void klient.invalidateQueries({ queryKey: votmed.koik, exact: true })
      notifications.show({ color: 'teal', title: 'Salvestatud', message: `f(x) = ${f.valem}` })
    },
  })
}

export function useMuuda(id: number) {
  const klient = useQueryClient()
  return useMutation({
    mutationFn: (p: FunktsiooniPaering) => api.muuda(id, p),
    onSuccess: (f) => {
      klient.setQueryData(votmed.yks(f.id), f)
      void klient.invalidateQueries({ queryKey: votmed.koik, exact: true })
      notifications.show({ color: 'teal', title: 'Muudetud', message: `f(x) = ${f.valem}` })
    },
  })
}

export function useKustuta() {
  const klient = useQueryClient()
  return useMutation({
    mutationFn: api.kustuta,
    onSuccess: (_, id) => {
      klient.removeQueries({ queryKey: votmed.yks(id) })
      void klient.invalidateQueries({ queryKey: votmed.koik, exact: true })
      notifications.show({ color: 'gray', title: 'Kustutatud', message: 'Funktsioon eemaldati andmebaasist.' })
    },
    onError: (viga) => notifications.show({ color: 'red', title: 'Kustutamine ebaõnnestus', message: viga.message }),
  })
}
