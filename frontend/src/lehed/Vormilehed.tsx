import { Alert, Loader, Stack, Text, Title } from '@mantine/core'
import { useParams, useSearchParams } from 'react-router'
import { useFunktsioon, useLoo, useMuuda } from '../api/paringud'
import { FunktsiooniVorm } from '../komponendid/FunktsiooniVorm'

export function UusFunktsioon() {
  const loo = useLoo()
  // teooria lehe „Proovi lahendajas“ lingid: /uus?valem=x^3&algus=-2&lopp=2
  const [parameetrid] = useSearchParams()
  const valem = parameetrid.get('valem')
  const arv = (nimi: string, vaikimisi: number) => {
    const v = Number(parameetrid.get(nimi))
    return parameetrid.has(nimi) && Number.isFinite(v) ? v : vaikimisi
  }
  const algvaartused = valem ? { valem, vahemikAlgus: arv('algus', -5), vahemikLopp: arv('lopp', 5) } : undefined
  return (
    <Stack gap="md">
      <div>
        <Title order={2}>Uus funktsioon</Title>
        <Text c="dimmed" size="sm">
          Sisesta valem – vastused ja graafik ilmuvad kohe.
        </Text>
      </div>
      <FunktsiooniVorm
        key={parameetrid.toString()}
        algvaartused={algvaartused}
        nupuTekst="Salvesta"
        salvestab={loo.isPending}
        onSalvesta={loo.mutateAsync}
      />
    </Stack>
  )
}

export function MuudaFunktsiooni() {
  const id = Number(useParams().id)
  const { data: f, isLoading, error } = useFunktsioon(id)
  const muuda = useMuuda(id)

  if (isLoading) return <Loader m="xl" />
  if (error || !f) return <Alert color="red" title="Funktsiooni ei leitud">{error?.message}</Alert>

  return (
    <Stack gap="md">
      <Title order={2}>Muuda funktsiooni</Title>
      <FunktsiooniVorm
        key={f.id}
        algvaartused={{ valem: f.valem, vahemikAlgus: f.vahemikAlgus, vahemikLopp: f.vahemikLopp }}
        nupuTekst="Salvesta muudatus"
        salvestab={muuda.isPending}
        onSalvesta={muuda.mutateAsync}
      />
    </Stack>
  )
}
