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
  /** Kas joonestada ka tuletised ja käänupunktid (kasutaja valis „Näita rohkem“). */
  koikDetailid?: boolean
}

/** Nii mitmeks lõiguks jagatakse vahemik; samm arvutatakse sellest automaatselt. */
const PUNKTE = 500

/** Graafik koos vahemiku valikuga; punktid arvutab server valitud vahemikus. */
export function GraafikuPaneel({ valem, algus, lopp, koikDetailid = false }: Props) {
  const [vahemik, setVahemik] = useState({ algus, lopp })
  // kui salvestatud/vormis olev vahemik muutub, joonestame uue vahemiku
  const [eelmised, setEelmised] = useState({ algus, lopp })
  if (eelmised.algus !== algus || eelmised.lopp !== lopp) {
    setEelmised({ algus, lopp })
    setVahemik({ algus, lopp })
  }
  const [viivitusega] = useDebouncedValue(vahemik, 300)

  const viga = viivitusega.algus >= viivitusega.lopp ? 'Vahemiku algus peab olema väiksem kui lõpp.' : null
  const samm = Math.max((viivitusega.lopp - viivitusega.algus) / PUNKTE, 0.0001)
  const graafik = useGraafik(viga ? null : { valem, ...viivitusega, samm })

  const muuda = (vali: 'algus' | 'lopp') => (v: string | number) =>
    typeof v === 'number' && setVahemik((eelmine) => ({ ...eelmine, [vali]: v }))

  return (
    <Card withBorder radius="md" padding="md">
      <Group justify="space-between" align="flex-end" mb="xs" wrap="wrap" gap="xs">
        <Text fw={600}>Graafik</Text>
        <Group gap="xs" wrap="nowrap">
          <NumberInput size="xs" w={92} label="x alates" value={vahemik.algus} onChange={muuda('algus')} step={1} min={-1000} max={1000} decimalScale={4} />
          <NumberInput size="xs" w={92} label="x kuni" value={vahemik.lopp} onChange={muuda('lopp')} step={1} min={-1000} max={1000} decimalScale={4} />
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
          koikDetailid={koikDetailid}
        />
      ) : (
        <Skeleton height={460} radius="md" animate={graafik.isLoading} />
      )}

      <Text size="xs" c="dimmed" mt={4}>
        {koikDetailid && "Pidev joon – f(x), katkendjoon – f'(x), punktiir – f''(x) (sisse lülitatav legendist). "}
        Hiirerattaga saab suurendada.
      </Text>
    </Card>
  )
}
