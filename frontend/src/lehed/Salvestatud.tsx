import { ActionIcon, Button, Card, Group, SimpleGrid, Skeleton, Stack, Text, TextInput, Title, Tooltip } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconPlus, IconSearch, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { useFunktsioonid, useKustuta } from '../api/paringud'
import type { FunktsiooniVastus } from '../api/tyybid'
import { Valem } from '../komponendid/Valem'
import klassid from './Avaleht.module.css'
import { kuupaev } from './vorming'

/** Kõik salvestatud funktsioonid kaartidena, uuemad eespool. */
export function Salvestatud() {
  const { data, isLoading, isError } = useFunktsioonid()
  const kustuta = useKustuta()
  const [otsing, setOtsing] = useState('')
  const nahtavad = [...(data ?? [])]
    .filter((f) => f.valem.toLowerCase().includes(otsing.trim().toLowerCase()))
    .sort((a, b) => (b.muudetudAeg ?? b.luodudAeg).localeCompare(a.muudetudAeg ?? a.luodudAeg))

  const kinnitaKustutamine = (f: FunktsiooniVastus) =>
    modals.openConfirmModal({
      title: 'Kustuta funktsioon',
      children: (
        <Text size="sm">
          Kas kustutada <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} />{' '}
          ja selle uurimise tulemused andmebaasist?
        </Text>
      ),
      labels: { confirm: 'Kustuta', cancel: 'Loobu' },
      confirmProps: { color: 'red' },
      onConfirm: () => kustuta.mutate(f.id),
    })

  return (
    <Stack gap="lg" maw={1100} mx="auto">
      <Group justify="space-between" align="flex-end">
        <div>
          <Title order={2}>Salvestatud funktsioonid</Title>
          <Text c="dimmed" size="sm">{data ? `${data.length} funktsiooni andmebaasis` : ' '}</Text>
        </div>
        <Group gap="xs">
          <TextInput
            placeholder="Otsi valemit…"
            leftSection={<IconSearch size={16} />}
            value={otsing}
            onChange={(e) => setOtsing(e.currentTarget.value)}
            w={220}
          />
          <Button component={Link} to="/uus" leftSection={<IconPlus size={18} />}>Uus</Button>
        </Group>
      </Group>

      {isError && <Text c="red">Funktsioone ei õnnestunud laadida.</Text>}
      {data && nahtavad.length === 0 && (
        <Text c="dimmed">{otsing ? 'Ühtegi valemit ei leitud.' : 'Salvestatud funktsioone pole.'}</Text>
      )}

      <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }}>
        {isLoading && Array.from({ length: 6 }, (_, i) => <Skeleton key={i} height={110} radius="md" />)}
        {nahtavad.map((f) => (
          <Card key={f.id} withBorder radius="md" className={`${klassid.kaart} ${klassid.kaarditaust}`}>
            <Link to={`/funktsioon/${f.id}`} className={klassid.katteLink} aria-label={`Ava f(x) = ${f.valem}`} />
            <Group justify="space-between" align="flex-start" wrap="nowrap" gap="xs">
              <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} fz="lg" />
              <Tooltip label="Kustuta">
                <ActionIcon
                  className={klassid.kaardiNupp}
                  variant="subtle"
                  color="red"
                  aria-label={`Kustuta f(x) = ${f.valem}`}
                  loading={kustuta.isPending && kustuta.variables === f.id}
                  onClick={() => kinnitaKustutamine(f)}
                >
                  <IconTrash size={18} />
                </ActionIcon>
              </Tooltip>
            </Group>
            <Text size="xs" c="dimmed" mt={6} lineClamp={2}>
              {f.ekstreemumid === 'puuduvad' ? 'Ekstreemumid puuduvad' : f.ekstreemumid}
            </Text>
            <Text size="xs" c="dimmed" mt={8}>{kuupaev(f.muudetudAeg ?? f.luodudAeg)}</Text>
          </Card>
        ))}
      </SimpleGrid>
    </Stack>
  )
}
