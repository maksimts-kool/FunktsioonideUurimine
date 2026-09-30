import { Button, Card, Group, SimpleGrid, Skeleton, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import { IconChartLine, IconDatabase, IconMathFunction, IconPlus } from '@tabler/icons-react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'

const OMADUSED = [
  {
    ikoon: IconMathFunction,
    pealkiri: 'Sümbolarvutus serveris',
    tekst: 'MathNet.Symbolics leiab tuletised; nullkohad ja ekstreemumid arvutatakse täpselt (√3, π/2, 1/e).',
  },
  {
    ikoon: IconChartLine,
    pealkiri: 'Graafik',
    tekst: "f(x) ja tuletis f'(x) samal teljestikul, nullkohad, ekstreemumid, käänupunktid ja asümptoodid.",
  },
  {
    ikoon: IconDatabase,
    pealkiri: 'Andmebaas',
    tekst: 'Uurimised salvestatakse EF Core ja SQLite abil – loo, muuda ja kustuta.',
  },
]

export function Avaleht() {
  const { data, isLoading } = useFunktsioonid()

  return (
    <Stack gap="lg" maw={1100}>
      <div>
        <Title order={2}>Funktsioonide uurimine</Title>
        <Text c="dimmed">
          Sisesta funktsioon, mille omadused arvutatakse automaatselt: määramispiirkond, nullkohad, tuletis,
          kriitilised punktid, ekstreemumid, monotoonsus ja kumerus.
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
