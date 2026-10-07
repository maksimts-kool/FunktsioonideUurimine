import {
  ActionIcon,
  Anchor,
  AppShell,
  Badge,
  Box,
  Burger,
  Button,
  Group,
  Loader,
  NavLink,
  ScrollArea,
  Tabs,
  Text,
  Title,
  Tooltip,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import {
  IconBook2,
  IconDeviceFloppy,
  IconHome,
  IconMathFunction,
  IconMoon,
  IconPlus,
  IconSun,
  type Icon,
} from '@tabler/icons-react'
import { Link, Outlet, useLocation, useMatch, useNavigate } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'
import { TeooriaSisukord } from './Teooria'
import { kuupaev } from './vorming'

type Vaade = 'avaleht' | 'lahendaja' | 'teooria' | 'salvestatud'

const SAKID: { vaade: Vaade; tee: string; nimi: string; ikoon: Icon }[] = [
  { vaade: 'avaleht', tee: '/', nimi: 'Avaleht', ikoon: IconHome },
  { vaade: 'lahendaja', tee: '/uus', nimi: 'Lahendaja', ikoon: IconMathFunction },
  { vaade: 'teooria', tee: '/teooria', nimi: 'Teooria', ikoon: IconBook2 },
  { vaade: 'salvestatud', tee: '/funktsioonid', nimi: 'Salvestatud', ikoon: IconDeviceFloppy },
]

/** Milline sakk on aktiivne; funktsiooni vaatamine ja muutmine kuuluvad lahendaja alla. */
function vaadeTeest(tee: string): Vaade | null {
  if (tee === '/') return 'avaleht'
  if (tee === '/uus' || tee.startsWith('/funktsioon/')) return 'lahendaja'
  if (tee === '/teooria') return 'teooria'
  if (tee === '/funktsioonid') return 'salvestatud'
  return null
}

/** Nii mitu viimati loodud/muudetud funktsiooni näidatakse lahendaja külgribas. */
const VIIMASEID = 8

/** Lahendaja külgriba: viimased funktsioonid; kõik on saki „Salvestatud“ all. */
function ViimasedFunktsioonid({ sulge }: { sulge: () => void }) {
  const { data, isLoading, isError } = useFunktsioonid()
  const aktiivne = useMatch('/funktsioon/:id/*')?.params.id

  const viimased = [...(data ?? [])]
    .sort((a, b) => (b.muudetudAeg ?? b.luodudAeg).localeCompare(a.muudetudAeg ?? a.luodudAeg))
    .slice(0, VIIMASEID)

  return (
    <>
      <Text size="xs" fw={700} tt="uppercase" c="dimmed" mb="xs" px={4}>Viimased funktsioonid</Text>
      <ScrollArea style={{ flex: 1 }} type="auto" offsetScrollbars>
        {isLoading && <Loader size="sm" m="md" />}
        {isError && <Text c="red" size="sm">Funktsioone ei õnnestunud laadida.</Text>}
        {data && viimased.length === 0 && <Text c="dimmed" size="sm" p="xs">Salvestatud funktsioone pole.</Text>}
        {viimased.map((f) => (
          <NavLink
            key={f.id}
            component={Link}
            to={`/funktsioon/${f.id}`}
            onClick={sulge}
            active={aktiivne === String(f.id)}
            label={<Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} />}
            description={kuupaev(f.muudetudAeg ?? f.luodudAeg)}
            styles={{ label: { overflow: 'hidden', textOverflow: 'ellipsis' } }}
            style={{ borderRadius: 'var(--mantine-radius-sm)' }}
          />
        ))}
      </ScrollArea>
      <Button
        component={Link}
        to="/funktsioonid"
        onClick={sulge}
        variant="light"
        mt="xs"
        fullWidth
        leftSection={<IconDeviceFloppy size={18} />}
        rightSection={data ? <Badge size="sm" variant="filled" circle={data.length < 10}>{data.length}</Badge> : null}
      >
        Kõik salvestatud
      </Button>
    </>
  )
}

export function Paigutus() {
  const [avatud, { toggle, close }] = useDisclosure()
  const { setColorScheme } = useMantineColorScheme()
  const tume = useComputedColorScheme('light') === 'dark'
  const navigeeri = useNavigate()
  const vaade = vaadeTeest(useLocation().pathname)

  // külgriba: lahendajas salvestatud funktsioonid, teoorias sisukord; avalehel ja nimekirjas pole vaja
  const kylgriba =
    vaade === 'lahendaja' ? (
      <>
        <Button component={Link} to="/uus" onClick={close} leftSection={<IconPlus size={18} />} mb="sm" fullWidth>
          Uus funktsioon
        </Button>
        <ViimasedFunktsioonid sulge={close} />
      </>
    ) : vaade === 'teooria' ? (
      <>
        <Text size="xs" fw={700} tt="uppercase" c="dimmed" mb="xs" px={4}>Sisukord</Text>
        <ScrollArea style={{ flex: 1 }} type="auto">
          <TeooriaSisukord onValik={close} />
        </ScrollArea>
      </>
    ) : null

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={kylgriba ? { width: 300, breakpoint: 'sm', collapsed: { mobile: !avatud } } : undefined}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap" gap="xs">
          <Group gap="sm" wrap="nowrap">
            {kylgriba && <Burger opened={avatud} onClick={toggle} hiddenFrom="sm" size="sm" aria-label="Külgriba" />}
            <Anchor component={Link} to="/" underline="never" c="inherit">
              <Group gap={8} wrap="nowrap">
                <IconMathFunction size={28} color="var(--mantine-color-indigo-6)" />
                <Title order={4} visibleFrom="lg">Funktsioonide uurimine</Title>
              </Group>
            </Anchor>
          </Group>

          <Tabs
            value={vaade}
            onChange={(v) => {
              const sakk = SAKID.find((s) => s.vaade === v)
              if (sakk) navigeeri(sakk.tee)
              close()
            }}
            h="100%"
            styles={{ list: { height: '100%', flexWrap: 'nowrap', '--tab-border-color': 'transparent' }, tab: { height: '100%' } }}
          >
            <Tabs.List>
              {SAKID.map(({ vaade: v, nimi, ikoon: Ikoon }) => (
                <Tabs.Tab key={v} value={v} leftSection={<Ikoon size={18} />} aria-label={nimi} px={{ base: 'xs', sm: 'md' }}>
                  <Box component="span" visibleFrom="sm">{nimi}</Box>
                </Tabs.Tab>
              ))}
            </Tabs.List>
          </Tabs>

          <Tooltip label={tume ? 'Hele teema' : 'Tume teema'}>
            <ActionIcon
              variant="default"
              size="lg"
              aria-label="Vaheta teemat"
              onClick={() => setColorScheme(tume ? 'light' : 'dark')}
            >
              {tume ? <IconSun size={18} /> : <IconMoon size={18} />}
            </ActionIcon>
          </Tooltip>
        </Group>
      </AppShell.Header>

      {kylgriba && (
        <AppShell.Navbar p="sm" style={{ display: 'flex', flexDirection: 'column' }}>
          {kylgriba}
        </AppShell.Navbar>
      )}

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  )
}
