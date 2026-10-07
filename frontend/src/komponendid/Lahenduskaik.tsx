import { Accordion, Alert, Badge, Box, Card, Group, Paper, ScrollArea, Skeleton, Stack, Text, ThemeIcon, Tooltip } from '@mantine/core'
import { IconAlertTriangle, IconCircleCheck, IconListNumbers } from '@tabler/icons-react'
import { useEffect, useState } from 'react'
import { useLahenduskaik } from '../api/paringud'
import type { FunktsiooniPaering, LahenduseOsa, Samm } from '../api/tyybid'
import { AVA_SUNDMUS, lahenduseId } from './lahendus'
import { MargitabelVaade } from './MargitabelVaade'
import { RikasTekst, Valem } from './Valem'


function SammuVaade({ samm }: { samm: Samm }) {
  return (
    <Stack gap={6}>
      {samm.tekst && (
        <Text size="sm" lh={1.65}>
          <RikasTekst tekst={samm.tekst} />
        </Text>
      )}
      {samm.valem && (
        <ScrollArea type="auto" offsetScrollbars="x">
          <Valem latex={samm.valem} blokk fz="md" px="sm" style={{ margin: 0 }} />
        </ScrollArea>
      )}
      {samm.tabel && <MargitabelVaade tabel={samm.tabel} />}
    </Stack>
  )
}

function OsaVaade({ osa }: { osa: LahenduseOsa }) {
  return (
    <Stack gap="sm">
      {osa.sammud.map((s, i) => (
        <SammuVaade key={i} samm={s} />
      ))}
      <Paper withBorder p="sm" radius="md" bg="var(--mantine-primary-color-light)" style={{ borderColor: 'var(--mantine-primary-color-light-hover)' }}>
        <Group gap="xs" wrap="nowrap" align="flex-start">
          <ThemeIcon variant="transparent" size="sm" mt={2}><IconCircleCheck size={18} /></ThemeIcon>
          <Stack gap={2}>
            <Text size="xs" fw={700} tt="uppercase" c="dimmed">Vastus</Text>
            {osa.vastused.map((v) => (
              <Text key={v} size="sm" fw={500}>{v}</Text>
            ))}
          </Stack>
        </Group>
      </Paper>
    </Stack>
  )
}

/**
 * Samm-sammuline lahenduskäik: kuidas iga vastus leiti – tingimused, lahendatud võrrandid,
 * diferentseerimisreeglid ja märgitabelid. Arvutused teeb server (POST /api/lahenduskaik).
 */
export function Lahenduskaik({ paering }: { paering: FunktsiooniPaering }) {
  const { data, isLoading, isError, error } = useLahenduskaik(paering)
  // null = kõik osad lahti (vaikimisi); kasutaja saab neid sulgeda
  const [avatud, setAvatud] = useState<string[] | null>(null)
  const koik = data?.osad.map((o) => o.voti) ?? []
  const vaartus = avatud ?? koik

  useEffect(() => {
    const ava = (e: Event) => {
      const voti = (e as CustomEvent<string>).detail
      setAvatud((eelmine) => {
        const praegu = eelmine ?? koik
        return praegu.includes(voti) ? praegu : [...praegu, voti]
      })
    }
    window.addEventListener(AVA_SUNDMUS, ava)
    return () => window.removeEventListener(AVA_SUNDMUS, ava)
  })

  return (
    <Card withBorder radius="md" padding="md">
      <Group gap="sm" mb="md" wrap="nowrap">
        <ThemeIcon variant="light" size="lg"><IconListNumbers size={20} /></ThemeIcon>
        <div>
          <Text fw={600}>Lahenduskäik</Text>
          <Text size="xs" c="dimmed">Kuidas iga vastus leiti – samm-sammult, nagu käsitsi lahendades</Text>
        </div>
      </Group>

      {isLoading && (
        <Stack gap="xs">
          {Array.from({ length: 4 }, (_, i) => <Skeleton key={i} height={52} radius="md" />)}
        </Stack>
      )}
      {isError && <Alert color="red" icon={<IconAlertTriangle size={18} />}>{error.message}</Alert>}

      {data && (
        <Accordion multiple variant="separated" radius="md" value={vaartus} onChange={setAvatud} chevronPosition="right">
          {data.osad.map((osa, i) => (
            <Accordion.Item key={osa.voti} value={osa.voti} id={lahenduseId(osa.voti)} style={{ scrollMarginTop: 76 }}>
              <Accordion.Control>
                <Group gap="sm" wrap="nowrap">
                  <Badge variant="filled" circle size="lg">{i + 1}</Badge>
                  <Box style={{ minWidth: 0 }}>
                    <Text fw={600} size="sm">{osa.pealkiri}</Text>
                  </Box>
                  {osa.numbriline && (
                    <Tooltip label="Võrrandit ei saa algebraliselt lahendada – tulemus on leitud numbriliselt uuritavas vahemikus" multiline w={260}>
                      <Badge size="xs" variant="light" color="yellow" style={{ flexShrink: 0 }}>numbriliselt</Badge>
                    </Tooltip>
                  )}
                </Group>
              </Accordion.Control>
              <Accordion.Panel>
                <OsaVaade osa={osa} />
              </Accordion.Panel>
            </Accordion.Item>
          ))}
        </Accordion>
      )}
    </Card>
  )
}
