import { Alert, Card, Group, NumberInput, Skeleton, Text } from '@mantine/core'
import { useDebouncedValue } from '@mantine/hooks'
import { IconAlertTriangle } from '@tabler/icons-react'
import { useState } from 'react'
import { useGraafik } from '../api/paringud'
import { FunktsiooniGraafik } from './FunktsiooniGraafik'

interface Props {
  valem: string
  algus: number
  lopp: number
}

const MAX_PUNKTE = 20000

/** Graafik koos vahemiku ja sammu valikuga; punktid arvutab server valitud vahemikus antud sammuga. */
export function GraafikuPaneel({ valem, algus, lopp }: Props) {
  const [vahemik, setVahemik] = useState({ algus, lopp, samm: 0.05 })
  // kui salvestatud/vormis olev vahemik muutub, joonestame uue vahemiku (samm jääb alles)
  const [eelmised, setEelmised] = useState({ algus, lopp })
  if (eelmised.algus !== algus || eelmised.lopp !== lopp) {
    setEelmised({ algus, lopp })
    setVahemik((v) => ({ ...v, algus, lopp }))
  }
  const [viivitusega] = useDebouncedValue(vahemik, 300)

  const viga =
    viivitusega.algus >= viivitusega.lopp
      ? 'Vahemiku algus peab olema väiksem kui lõpp.'
      : viivitusega.samm <= 0 || (viivitusega.lopp - viivitusega.algus) / viivitusega.samm > MAX_PUNKTE
        ? `Samm on liiga väike (kuni ${MAX_PUNKTE} punkti).`
        : null
  const graafik = useGraafik(viga ? null : { valem, ...viivitusega })

  const muuda = (vali: 'algus' | 'lopp' | 'samm') => (v: string | number) =>
    typeof v === 'number' && setVahemik((eelmine) => ({ ...eelmine, [vali]: v }))

  return (
    <Card withBorder radius="md" padding="md">
      <Group justify="space-between" align="flex-end" mb="xs" wrap="wrap" gap="xs">
        <Text fw={600}>Graafik</Text>
        <Group gap="xs" wrap="nowrap">
          <NumberInput size="xs" w={92} label="x algus" value={vahemik.algus} onChange={muuda('algus')} step={1} min={-1000} max={1000} decimalScale={4} />
          <NumberInput size="xs" w={92} label="x lõpp" value={vahemik.lopp} onChange={muuda('lopp')} step={1} min={-1000} max={1000} decimalScale={4} />
          <NumberInput size="xs" w={80} label="samm" value={vahemik.samm} onChange={muuda('samm')} step={0.01} min={0.0001} decimalScale={4} />
        </Group>
      </Group>

      {viga && <Alert color="yellow" icon={<IconAlertTriangle size={18} />} mb="xs">{viga}</Alert>}
      {graafik.isError && (
        <Alert color="red" icon={<IconAlertTriangle size={18} />} mb="xs">{graafik.error.message}</Alert>
      )}

      {graafik.data ? (
        <FunktsiooniGraafik
          andmed={graafik.data}
          algus={viivitusega.algus}
          lopp={viivitusega.lopp}
          laadib={graafik.isFetching}
        />
      ) : (
        <Skeleton height={460} radius="md" animate={graafik.isLoading} />
      )}

      <Text size="xs" c="dimmed" mt={4}>
        Pidev joon – f(x), katkendjoon – f'(x), punktiir – f''(x) (sisse lülitatav legendist). Hiirerattaga saab suurendada.
      </Text>
    </Card>
  )
}
