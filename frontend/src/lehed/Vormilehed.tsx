import { Stack, Text, Title } from '@mantine/core'
import { useSearchParams } from 'react-router'
import { useLoo } from '../api/paringud'
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
