import { Table, Text } from '@mantine/core'
import type { Margitabel } from '../api/tyybid'
import { Valem } from './Valem'

const MARGI_VARV: Record<string, string> = {
  '+': 'teal',
  '−': 'red',
  '0': 'gray',
  '∄': 'dimmed',
}

const pealkiri = { background: 'var(--mantine-color-default-hover)', whiteSpace: 'nowrap' } as const

/**
 * Märgitabel (intervallmeetod): veerud on vahemikud ja murdepunktid, read – x, märk, tähendus
 * (nool, max/min, ∪/∩) ning testpunkti arvutus.
 */
export function MargitabelVaade({ tabel }: { tabel: Margitabel }) {
  const { veerud } = tabel
  const kontrollid = veerud.some((v) => v.kontroll)

  return (
    <Table.ScrollContainer minWidth={Math.max(veerud.length * 84, 300)} type="native">
      <Table withTableBorder withColumnBorders ta="center" verticalSpacing={6} horizontalSpacing="xs" fz="sm">
        <Table.Tbody>
          <Table.Tr>
            <Table.Th style={pealkiri}><Valem latex="x" /></Table.Th>
            {veerud.map((v, i) => (
              <Table.Td key={i} style={{ whiteSpace: 'nowrap' }} bg={v.vahemik ? undefined : 'var(--mantine-color-default-hover)'}>
                <Valem latex={v.x} />
              </Table.Td>
            ))}
          </Table.Tr>
          <Table.Tr>
            <Table.Th style={pealkiri}><Valem latex={tabel.margiRida} /></Table.Th>
            {veerud.map((v, i) => (
              <Table.Td key={i} bg={v.vahemik ? undefined : 'var(--mantine-color-default-hover)'}>
                <Text component="span" fw={700} fz="lg" c={MARGI_VARV[v.mark]}>{v.mark}</Text>
              </Table.Td>
            ))}
          </Table.Tr>
          {tabel.tahenduseRida && (
            <Table.Tr>
              <Table.Th style={pealkiri}><Valem latex={tabel.tahenduseRida} /></Table.Th>
              {veerud.map((v, i) => (
                <Table.Td key={i} bg={v.vahemik ? undefined : 'var(--mantine-color-default-hover)'} fz="lg">
                  {v.tahendus && <Valem latex={v.tahendus} />}
                </Table.Td>
              ))}
            </Table.Tr>
          )}
          {kontrollid && (
            <Table.Tr>
              <Table.Th style={pealkiri}>
                <Text size="xs" c="dimmed">testpunkt</Text>
              </Table.Th>
              {veerud.map((v, i) => (
                <Table.Td key={i} bg={v.vahemik ? undefined : 'var(--mantine-color-default-hover)'}>
                  {v.kontroll && <Valem latex={v.kontroll} fz="xs" c="dimmed" />}
                </Table.Td>
              ))}
            </Table.Tr>
          )}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  )
}
