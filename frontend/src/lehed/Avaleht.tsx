import { Button, Card, Group, SimpleGrid, Skeleton, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import { IconChartLine, IconDeviceFloppy, IconMathFunction, IconPlus } from '@tabler/icons-react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'

const OMADUSED = [
  {
    ikoon: IconMathFunction,
    pealkiri: 'Täpsed vastused',
    tekst: 'Nullkohad ja ekstreemumid leitakse täpselt, nt √3, π/2 või 1/e – mitte ligikaudsete kümnendmurdudena.',
  },
  {
    ikoon: IconChartLine,
    pealkiri: 'Graafik',
    tekst: 'Funktsiooni graafik, millele on märgitud nullkohad ja ekstreemumid.',
  },
  {
    ikoon: IconDeviceFloppy,
    pealkiri: 'Salvestamine',
    tekst: 'Uuritud funktsioonid jäävad alles – neid saab hiljem uuesti vaadata, muuta või kustutada.',
  },
]

export function Avaleht() {
  const { data, isLoading } = useFunktsioonid()

  return (
    <Stack gap="lg" maw={1100}>
      <div>
        <Title order={2}>Funktsioonide uurimine</Title>
        <Text c="dimmed">
          Sisesta funktsioon ja saad kohe vastused: määramispiirkond, nullkohad, ekstreemumid ning kasvamis- ja
          kahanemisvahemikud. Rohkem detaile saab vaadata nupust „Näita rohkem“.
        </Text>
        <Button component={Link} to="/uus" mt="md" leftSection={<IconPlus size={18} />}>
          Uuri uut funktsiooni
        </Button>
      </div>

      <SimpleGrid cols={{ base: 1, sm: 3 }}>
        {OMADUSED.map(({ ikoon: Ikoon, pealkiri, tekst }) => (
          <Card key={pealkiri} withBorder radius="md">
            <ThemeIcon variant="light" size="lg" mb="xs"><Ikoon size={20} /></ThemeIcon>
            <Text fw={600}>{pealkiri}</Text>
            <Text size="sm" c="dimmed">{tekst}</Text>
          </Card>
        ))}
      </SimpleGrid>

      <div>
        <Title order={4} mb="sm">Salvestatud funktsioonid</Title>
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }}>
          {isLoading && Array.from({ length: 3 }, (_, i) => <Skeleton key={i} height={96} radius="md" />)}
          {data?.map((f) => (
            <Card key={f.id} withBorder radius="md" component={Link} to={`/funktsioon/${f.id}`} style={{ textDecoration: 'none' }}>
              <Group justify="space-between" wrap="nowrap">
                <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} fz="lg" />
              </Group>
              <Text size="xs" c="dimmed" mt={6} lineClamp={2}>
                {f.ekstreemumid === 'puuduvad' ? 'Ekstreemumid puuduvad' : f.ekstreemumid}
              </Text>
            </Card>
          ))}
        </SimpleGrid>
      </div>
    </Stack>
  )
}
