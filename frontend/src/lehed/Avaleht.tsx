import { Anchor, Badge, Card, Group, SimpleGrid, Skeleton, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import {
  IconApi,
  IconArrowRight,
  IconBook2,
  IconDeviceFloppy,
  IconListCheck,
  IconMathFunction,
  IconTable,
  type Icon,
} from '@tabler/icons-react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'
import klassid from './Avaleht.module.css'

interface Kaart {
  ikoon: Icon
  pealkiri: string
  kirjeldus: string
  link: string
  varv: string
  valine?: boolean
}

const PEAMISED: Kaart[] = [
  {
    ikoon: IconMathFunction,
    pealkiri: 'Funktsiooni lahendaja',
    kirjeldus:
      'Sisesta valem – saad määramispiirkonna, nullkohad, ekstreemumid, kumeruse, graafiku ja samm-sammulise lahenduskäigu.',
    link: '/uus',
    varv: 'indigo',
  },
  {
    ikoon: IconBook2,
    pealkiri: 'Teooria',
    kirjeldus: 'Käsiraamat: määramispiirkond, tuletis, ekstreemumid, kumerus ja käänupunktid koos näidetega.',
    link: '/teooria',
    varv: 'grape',
  },
  {
    ikoon: IconDeviceFloppy,
    pealkiri: 'Salvestatud funktsioonid',
    kirjeldus: 'Kõik andmebaasi salvestatud uurimised – ava, muuda või kustuta.',
    link: '/funktsioonid',
    varv: 'teal',
  },
]

const KIIRLINGID: Kaart[] = [
  { ikoon: IconTable, pealkiri: 'Tuletiste tabel', kirjeldus: 'Põhifunktsioonide tuletised ja reeglid', link: '/teooria#tuletiste-tabel', varv: 'orange' },
  { ikoon: IconListCheck, pealkiri: 'Uurimise skeem', kirjeldus: 'Järjekord ja täisnäide x³ − 3x', link: '/teooria#skeem', varv: 'cyan' },
  { ikoon: IconApi, pealkiri: 'API dokumentatsioon', kirjeldus: 'REST API (OpenAPI + Scalar)', link: '/api/docs', varv: 'gray', valine: true },
]

function LingiKaart({ k, suur, lisa }: { k: Kaart; suur?: boolean; lisa?: React.ReactNode }) {
  const sisu = (
    <Stack gap={suur ? 'md' : 6} h="100%">
      <Group justify="space-between" wrap="nowrap" align="flex-start">
        <ThemeIcon variant="light" color={k.varv} size={suur ? 56 : 40} radius="md">
          <k.ikoon size={suur ? 32 : 22} stroke={1.6} />
        </ThemeIcon>
        {lisa}
      </Group>
      <div style={{ flex: 1 }}>
        <Text fw={700} size={suur ? 'xl' : 'md'}>{k.pealkiri}</Text>
        <Text size="sm" c="dimmed" mt={4} lh={1.55}>{k.kirjeldus}</Text>
      </div>
      {suur && (
        <Group gap={6} c={k.varv} fw={600} fz="sm">
          Ava <IconArrowRight size={16} className={klassid.nool} />
        </Group>
      )}
    </Stack>
  )
  const omadused = { withBorder: true, radius: 'lg', padding: suur ? 'xl' : 'md', className: klassid.kaart } as const
  // API dokumentatsiooni serveerib backend, mitte React Router
  return k.valine ? (
    <Card {...omadused} component="a" href={k.link}>{sisu}</Card>
  ) : (
    <Card {...omadused} component={Link} to={k.link}>{sisu}</Card>
  )
}

export function Avaleht() {
  const { data, isLoading } = useFunktsioonid()
  const viimased = [...(data ?? [])]
    .sort((a, b) => (b.muudetudAeg ?? b.luodudAeg).localeCompare(a.muudetudAeg ?? a.luodudAeg))
    .slice(0, 3)

  return (
    <Stack gap={36} maw={1100} py="md">
      <Stack gap="xs">
        <Title order={1}>Funktsioonide uurimine</Title>
        <Text c="dimmed" size="lg" maw={680}>
          Uuri funktsiooni tuletise abil ja vaata, kuidas iga vastus leiti – või loe teooriat, kuidas seda ise teha.
        </Text>
      </Stack>

      <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="lg">
        {PEAMISED.map((k) => (
          <LingiKaart
            key={k.link}
            k={k}
            suur
            lisa={
              k.link === '/funktsioonid' && data ? (
                <Badge variant="light" color="teal" size="lg">{data.length}</Badge>
              ) : null
            }
          />
        ))}
      </SimpleGrid>

      <SimpleGrid cols={{ base: 1, xs: 3 }} spacing="md">
        {KIIRLINGID.map((k) => (
          <LingiKaart key={k.link} k={k} />
        ))}
      </SimpleGrid>

      <div>
        <Group justify="space-between" mb="sm">
          <Title order={4}>Viimati salvestatud</Title>
          <Anchor component={Link} to="/funktsioonid" size="sm">Vaata kõiki →</Anchor>
        </Group>
        <SimpleGrid cols={{ base: 1, sm: 3 }}>
          {isLoading && Array.from({ length: 3 }, (_, i) => <Skeleton key={i} height={84} radius="md" />)}
          {viimased.map((f) => (
            <Card key={f.id} withBorder radius="md" component={Link} to={`/funktsioon/${f.id}`} className={klassid.kaart}>
              <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} fz="lg" />
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
