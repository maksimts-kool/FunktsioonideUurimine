import { Button, Card, Group, SimpleGrid, Skeleton, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import { IconChartLine, IconDeviceFloppy, IconMathFunction, IconPlus } from '@tabler/icons-react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'

const OMADUSED = [
  {
    ikoon: IconMathFunction,
    pealkiri: 'Täpsed vastused'
  },
  {
    ikoon: IconChartLine,
    pealkiri: 'Graafik'
  },
  {
    ikoon: IconDeviceFloppy,
    pealkiri: 'Salvestamine'
  },
]

export function Avaleht() {
  const { data, isLoading } = useFunktsioonid()

  return (
    <Stack gap="lg" maw={1100}>
      <div>
        <Title order={2}>Funktsioonide uurimine</Title>
        <Button component={Link} to="/uus" mt="md" leftSection={<IconPlus size={18} />}>
          Uuri uut funktsiooni
        </Button>
      </div>

      <SimpleGrid cols={{ base: 1, sm: 3 }}>
        {OMADUSED.map(({ ikoon: Ikoon, pealkiri }) => (
          <Card key={pealkiri} withBorder radius="md">
            <ThemeIcon variant="light" size="xl" mb="sm"><Ikoon size={50} /></ThemeIcon>
            <Text fw={600} size="lg">{pealkiri}</Text>
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
