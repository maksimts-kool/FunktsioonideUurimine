import { Badge, Button, Card, Group, Stack, Table, Text, Tooltip } from '@mantine/core'
import { IconChevronDown, IconChevronUp } from '@tabler/icons-react'
import type { Omadused } from '../api/tyybid'
import { Valem } from './Valem'

const VAHEMIKU_MARKUS = / \(vahemikus \[[^\]]*\]\)$/

/** Jagab "; " kohalt, kuid mitte sulgude sees: "X↑ = (-∞; -1); X↓ = (-1; 1)" → 2 osa. */
function jaga(tekst: string) {
  const osad: string[] = []
  let sugavus = 0
  let algus = 0
  for (let i = 0; i < tekst.length; i++) {
    const m = tekst[i]
    if ('([{'.includes(m)) sugavus++
    else if (')]}'.includes(m)) sugavus--
    else if (m === ';' && sugavus === 0 && tekst[i + 1] === ' ') {
      osad.push(tekst.slice(algus, i))
      algus = i + 2
    }
  }
  osad.push(tekst.slice(algus))
  return osad
}

/** "x₁ = -1; x₂ = 1 (vahemikus [-5; 5])" → eraldi read + märkus, et tulemus on leitud numbriliselt. */
function Vaartus({ tekst }: { tekst: string }) {
  const markus = tekst.match(VAHEMIKU_MARKUS)?.[0]
  const osad = jaga(tekst.replace(VAHEMIKU_MARKUS, ''))
  return (
    <Stack gap={2}>
      {osad.map((osa) => (
        <Text key={osa} size="sm" c={osa === 'puuduvad' ? 'dimmed' : undefined}>
          {osa}
        </Text>
      ))}
      {markus && (
        <Tooltip label="Mittepolünoomilise funktsiooni omadused on leitud numbriliselt ainult uuritavas vahemikus" multiline w={260}>
          <Badge size="xs" variant="light" color="yellow" w="fit-content">
            {markus.trim().slice(1, -1)}
          </Badge>
        </Tooltip>
      )}
    </Stack>
  )
}

interface Rida {
  nimi: string
  tahis: string
  tekst: string
  /** Tuletise read kuvatakse KaTeX-iga. */
  latex?: string | null
}

interface Props {
  omadused: Omadused
  pealkiri?: string
  /** Kas kuvada ka tuletised, käänupunktid jm (muidu ainult põhivastused). */
  koikDetailid: boolean
  onKoikDetailid: (v: boolean) => void
}

/** Põhivastused on kohe näha; ülejäänud omadused avanevad nupust „Näita rohkem“. */
export function OmadusteTabel({ omadused: o, pealkiri = 'Vastused', koikDetailid, onKoikDetailid }: Props) {
  const pohiread: Rida[] = [
    { nimi: 'Määramispiirkond', tahis: 'X', tekst: o.maaramispiirkond },
    { nimi: 'Nullkohad', tahis: 'X₀', tekst: o.nullkohad },
    { nimi: 'Ekstreemumid', tahis: 'yₑ', tekst: o.ekstreemumid },
    { nimi: 'Kasvamis- ja kahanemisvahemikud', tahis: 'X↑, X↓', tekst: o.monotoonsus },
  ]
  const lisaread: Rida[] = [
    { nimi: 'Positiivsus- ja negatiivsuspiirkond', tahis: 'X⁺, X⁻', tekst: o.positiivsus },
    { nimi: 'Tuletis', tahis: "f'(x)", tekst: o.tuletis, latex: o.tuletisLatex },
    { nimi: 'Kriitilised punktid', tahis: 'xₑ', tekst: o.kriitilisedPunktid },
    { nimi: 'Teine tuletis', tahis: "f''(x)", tekst: o.teineTuletis, latex: o.teineTuletisLatex },
    { nimi: 'Käänupunktid', tahis: 'K', tekst: o.kaanupunktid },
    { nimi: 'Nõgusus (∪) ja kumerus (∩)', tahis: 'X∪, X∩', tekst: o.kumerus },
  ]
  const read = koikDetailid ? [...pohiread, ...lisaread] : pohiread

  return (
    <Card withBorder radius="md" padding="md">
      <Text fw={600} mb="xs">{pealkiri}</Text>
      <Table verticalSpacing="xs" striped highlightOnHover>
        <Table.Tbody>
          {read.map((r) => (
            <Table.Tr key={r.nimi}>
              <Table.Td w="42%" style={{ verticalAlign: 'top' }}>
                <Group gap={6} wrap="nowrap" align="flex-start">
                  <Badge
                    variant="light"
                    radius="sm"
                    miw={56}
                    style={{ textTransform: 'none', flexShrink: 0, overflow: 'visible' }}
                    styles={{ label: { overflow: 'visible' } }}
                  >
                    {r.tahis}
                  </Badge>
                  <Text size="sm" c="dimmed">{r.nimi}</Text>
                </Group>
              </Table.Td>
              <Table.Td>
                {r.latex !== undefined ? <Valem latex={r.latex} varuTekst={r.tekst} fz="md" /> : <Vaartus tekst={r.tekst} />}
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
      <Button
        variant="subtle"
        size="sm"
        mt="xs"
        w="fit-content"
        rightSection={koikDetailid ? <IconChevronUp size={16} /> : <IconChevronDown size={16} />}
        onClick={() => onKoikDetailid(!koikDetailid)}
      >
        {koikDetailid ? 'Näita vähem' : 'Näita rohkem'}
      </Button>
    </Card>
  )
}
