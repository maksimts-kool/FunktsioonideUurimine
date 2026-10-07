import { Badge, Card, Group, SimpleGrid, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import { IconApi, IconArrowRight, IconBook2, IconDeviceFloppy, IconMathFunction, type Icon } from '@tabler/icons-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import klassid from './Avaleht.module.css'

interface Kaart {
  ikoon: Icon
  pealkiri: string
  kirjeldus: string
  link: string
  varv: string
  /** Väline link (backend), mitte React Routeri leht. */
  valine?: boolean
}

const KAARDID: Kaart[] = [
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
    kirjeldus:
      'Käsiraamat: määramispiirkond, tuletis ja tuletiste tabel, ekstreemumid, kumerus, uurimise skeem ja näited.',
    link: '/teooria',
    varv: 'grape',
  },
  {
    ikoon: IconDeviceFloppy,
    pealkiri: 'Salvestatud funktsioonid',
    kirjeldus: 'Kõik andmebaasi salvestatud uurimised – ava või kustuta.',
    link: '/funktsioonid',
    varv: 'teal',
  },

]

/** Arendajale: laiusega kaart põhikaartide all. */
const API_KAART: Kaart = {
  ikoon: IconApi,
  pealkiri: 'API dokumentatsioon',
  kirjeldus: 'REST API kirjeldus (OpenAPI + Scalar): analüüs, graafik, lahenduskäik ja salvestamine.',
  link: '/api/docs',
  varv: 'orange',
  valine: true,
}

function LingiKaart({ k, lisa, lai = false }: { k: Kaart; lisa?: ReactNode; lai?: boolean }) {
  const sisu = lai ? (
    <Group wrap="nowrap" gap="lg">
      <ThemeIcon variant="light" color={k.varv} size={56} radius="md" style={{ flexShrink: 0 }}>
        <k.ikoon size={32} stroke={1.6} />
      </ThemeIcon>
      <div style={{ flex: 1, minWidth: 0 }}>
        <Text fw={700} size="xl">{k.pealkiri}</Text>
        <Text size="sm" c="dimmed" mt={4} lh={1.55}>{k.kirjeldus}</Text>
      </div>
      <Group gap={6} c={k.varv} fw={600} fz="sm" wrap="nowrap" visibleFrom="xs">
        Ava <IconArrowRight size={16} className={klassid.nool} />
      </Group>
    </Group>
  ) : (
    <Stack gap="md" h="100%">
      <Group justify="space-between" wrap="nowrap" align="flex-start">
        <ThemeIcon variant="light" color={k.varv} size={56} radius="md">
          <k.ikoon size={32} stroke={1.6} />
        </ThemeIcon>
        {lisa}
      </Group>
      <div style={{ flex: 1 }}>
        <Text fw={700} size="xl">{k.pealkiri}</Text>
        <Text size="sm" c="dimmed" mt={4} lh={1.55}>{k.kirjeldus}</Text>
      </div>
      <Group gap={6} c={k.varv} fw={600} fz="sm">
        Ava <IconArrowRight size={16} className={klassid.nool} />
      </Group>
    </Stack>
  )
  const omadused = { withBorder: true, radius: 'lg', padding: 'xl', className: klassid.kaart } as const
  // API dokumentatsiooni serveerib backend, mitte React Router
  return k.valine ? (
    <Card {...omadused} component="a" href={k.link}>{sisu}</Card>
  ) : (
    <Card {...omadused} component={Link} to={k.link}>{sisu}</Card>
  )
}

export function Avaleht() {
  const { data } = useFunktsioonid()

  return (
    <Stack gap={36} maw={1100} py="md" mx="auto">
      <Stack gap="xs">
        <Title order={1}>Funktsioonide uurimine</Title>
        <Text c="dimmed" size="lg" maw={680}>
          Uuri funktsiooni tuletise abil ja vaata, kuidas iga vastus leiti – või loe teooriat, kuidas seda ise teha.
        </Text>
      </Stack>

      <Stack gap="lg">
        <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="lg">
          {KAARDID.map((k) => (
            <LingiKaart
              key={k.link}
              k={k}
              lisa={
                k.link === '/funktsioonid' && data ? (
                  <Badge variant="light" color="teal" size="lg">{data.length}</Badge>
                ) : null
              }
            />
          ))}
        </SimpleGrid>
        <LingiKaart k={API_KAART} lai />
      </Stack>
    </Stack>
  )
}
