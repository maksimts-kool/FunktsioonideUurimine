import { Button, Card, Group, SimpleGrid, Skeleton, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { useFunktsioonid } from '../api/paringud'
import { Valem } from '../komponendid/Valem'
import klassid from './Avaleht.module.css'
import { kuupaev } from './vorming'

/** Kõik salvestatud funktsioonid kaartidena, uuemad eespool. */
export function Salvestatud() {
  const { data, isLoading, isError } = useFunktsioonid()
  const [otsing, setOtsing] = useState('')
  const nahtavad = [...(data ?? [])]
    .filter((f) => f.valem.toLowerCase().includes(otsing.trim().toLowerCase()))
    .sort((a, b) => (b.muudetudAeg ?? b.luodudAeg).localeCompare(a.muudetudAeg ?? a.luodudAeg))

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
          <Card key={f.id} withBorder radius="md" component={Link} to={`/funktsioon/${f.id}`} className={klassid.kaart}>
            <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={f.valem} fz="lg" />
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
