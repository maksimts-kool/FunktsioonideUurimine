import {
  ActionIcon,
  Anchor,
  AppShell,
  Burger,
  Button,
  Group,
  Loader,
  NavLink,
  ScrollArea,
  Text,
  TextInput,
  Title,
  Tooltip,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconApi, IconMathFunction, IconMoon, IconPlus, IconSearch, IconSun } from '@tabler/icons-react'
import { useState } from 'react'
import { Link, Outlet, useLocation, useMatch } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'
import { kuupaev } from './vorming'

function Funktsioonid({ sulge }: { sulge: () => void }) {
  const { data, isLoading, isError } = useFunktsioonid()
  const [otsing, setOtsing] = useState('')
  const aktiivne = useMatch('/funktsioon/:id/*')?.params.id

  const nahtavad = (data ?? []).filter((f) => f.valem.toLowerCase().includes(otsing.trim().toLowerCase()))

  return (
    <>
      <TextInput
        placeholder="Otsi valemit…"
        leftSection={<IconSearch size={16} />}
        value={otsing}
        onChange={(e) => setOtsing(e.currentTarget.value)}
        mb="xs"
      />
      <ScrollArea style={{ flex: 1 }} type="auto" offsetScrollbars>
        {isLoading && <Loader size="sm" m="md" />}
        {isError && <Text c="red" size="sm">Funktsioone ei õnnestunud laadida.</Text>}
        {data && nahtavad.length === 0 && (
          <Text c="dimmed" size="sm" p="xs">{otsing ? 'Ühtegi valemit ei leitud.' : 'Salvestatud funktsioone pole.'}</Text>
        )}
        {nahtavad.map((f) => (
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
      <Group justify="space-between" pt="xs">
        <Text size="xs" c="dimmed">{data ? `${data.length} funktsiooni` : ''}</Text>
        <Anchor href="/api/docs" target="_blank" size="xs" c="dimmed">
          <Group gap={4}><IconApi size={14} /> API</Group>
        </Anchor>
      </Group>
    </>
  )
}

export function Paigutus() {
  const [avatud, { toggle, close }] = useDisclosure()
  const { setColorScheme } = useMantineColorScheme()
  const tume = useComputedColorScheme('light') === 'dark'
  const asukoht = useLocation()

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !avatud } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="sm" wrap="nowrap">
            <Burger opened={avatud} onClick={toggle} hiddenFrom="sm" size="sm" aria-label="Menüü" />
            <Anchor component={Link} to="/" underline="never" c="inherit">
              <Group gap={8} wrap="nowrap">
                <IconMathFunction size={28} color="var(--mantine-color-indigo-6)" />
                <Title order={4} visibleFrom="xs">Funktsioonide uurimine</Title>
              </Group>
            </Anchor>
          </Group>
          <Group gap="xs" wrap="nowrap">
            <Button
              component={Link}
              to="/uus"
              leftSection={<IconPlus size={18} />}
              variant={asukoht.pathname === '/uus' ? 'light' : 'filled'}
            >
              Uus funktsioon
            </Button>
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
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="sm" style={{ display: 'flex', flexDirection: 'column' }}>
        <Funktsioonid sulge={close} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  )
}
