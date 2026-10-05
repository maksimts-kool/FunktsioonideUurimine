import {
  Alert,
  Button,
  Card,
  Grid,
  Group,
  List,
  NumberInput,
  Popover,
  Stack,
  Text,
  TextInput,
  UnstyledButton,
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { useDebouncedValue } from '@mantine/hooks'
import { IconAlertTriangle, IconDeviceFloppy, IconHelpCircle } from '@tabler/icons-react'
import { useState } from 'react'
import { useNavigate } from 'react-router'
import { ApiViga } from '../api/klient'
import { useAnaluus } from '../api/paringud'
import type { FunktsiooniPaering, FunktsiooniVastus } from '../api/tyybid'
import { GraafikuPaneel } from './GraafikuPaneel'
import { OmadusteTabel } from './OmadusteTabel'
import { Valem } from './Valem'

const NAITED: { valem: string; latex: string; algus: number; lopp: number }[] = [
  { valem: 'x^3 - 3x', latex: 'x^3 - 3x', algus: -3, lopp: 3 },
  { valem: 'x^2 - 4x + 3', latex: 'x^2 - 4x + 3', algus: -2, lopp: 6 },
  { valem: 'x^3 - 12x', latex: 'x^3 - 12x', algus: -5, lopp: 5 },
  { valem: '(x^2 - 1)/(x^2 + 1)', latex: '\\frac{x^2-1}{x^2+1}', algus: -5, lopp: 5 },
  { valem: 'x*e^(-x)', latex: 'x e^{-x}', algus: -1, lopp: 6 },
  { valem: 'x^2*ln(x)', latex: 'x^2 \\ln x', algus: 0, lopp: 2 },
  { valem: 'sqrt(4 - x^2)', latex: '\\sqrt{4 - x^2}', algus: -3, lopp: 3 },
  { valem: 'x^(2/3)', latex: 'x^{2/3}', algus: -3, lopp: 3 },
  { valem: 'sin(x)', latex: '\\sin x', algus: -6.3, lopp: 6.3 },
]

function Juhend() {
  return (
    <Popover width={320} position="bottom-end" withArrow shadow="md">
      <Popover.Target>
        <UnstyledButton aria-label="Valemi süntaks" c="dimmed" style={{ display: 'flex' }}>
          <IconHelpCircle size={18} />
        </UnstyledButton>
      </Popover.Target>
      <Popover.Dropdown>
        <Text size="sm" fw={600} mb={4}>Valemi kirjutamine</Text>
        <List size="xs" spacing={2}>
          <List.Item>muutuja <b>x</b>, tehted <b>+ − * / ^</b>, sulud</List.Item>
          <List.Item>korrutusmärgi võib jätta ära: <b>2x</b>, <b>3(x+1)</b></List.Item>
          <List.Item>sin, cos, tan (tg), cot (ctg), arcsin, arccos, arctan</List.Item>
          <List.Item>ln, lg (log), exp, sqrt, cbrt, sinh, cosh, tanh</List.Item>
          <List.Item>konstandid <b>pi</b> ja <b>e</b>; kümnendmurd punkti või komaga</List.Item>
        </List>
      </Popover.Dropdown>
    </Popover>
  )
}

interface Props {
  algvaartused?: FunktsiooniPaering
  nupuTekst: string
  salvestab: boolean
  onSalvesta: (p: FunktsiooniPaering) => Promise<FunktsiooniVastus>
}

/** Loomise ja muutmise vorm; valemit uuritakse juba kirjutamise ajal (eelvaade serverist). */
export function FunktsiooniVorm({ algvaartused, nupuTekst, salvestab, onSalvesta }: Props) {
  const navigeeri = useNavigate()
  const [koikDetailid, setKoikDetailid] = useState(false)
  const vorm = useForm<FunktsiooniPaering>({
    mode: 'controlled',
    initialValues: algvaartused ?? { valem: '', vahemikAlgus: -5, vahemikLopp: 5 },
    validate: {
      valem: (v) => (v.trim() === '' ? 'Sisesta funktsiooni valem.' : v.length > 200 ? 'Valem on liiga pikk.' : null),
      vahemikAlgus: (v, k) => (v < k.vahemikLopp ? null : 'Algus peab olema väiksem kui lõpp.'),
      vahemikLopp: (v) => (v >= -1000 && v <= 1000 ? null : 'Lubatud on lõik [-1000; 1000].'),
    },
  })

  const [eelvaade] = useDebouncedValue(vorm.values, 350)
  const eelvaadeKorras = eelvaade.valem.trim() !== '' && eelvaade.vahemikAlgus < eelvaade.vahemikLopp
  const analuus = useAnaluus(eelvaadeKorras ? eelvaade : null)
  const serveriViga = analuus.error instanceof ApiViga ? (analuus.error.valjad.valem ?? null) : null

  const salvesta = vorm.onSubmit(async (vaartused) => {
    try {
      const f = await onSalvesta({ ...vaartused, valem: vaartused.valem.trim() })
      navigeeri(`/funktsioon/${f.id}`)
    } catch (viga) {
      if (viga instanceof ApiViga) vorm.setErrors(viga.valjad)
    }
  })

  const eelvaateAndmed = analuus.isError ? null : analuus.data

  return (
    <Stack gap="md">
      <Card withBorder radius="md" padding="md" component="form" onSubmit={salvesta}>
        <Grid align="flex-start" gap="sm">
          <Grid.Col span={{ base: 12, md: 5 }}>
            <TextInput
              label={
                <Group gap={4} component="span">
                  Funktsiooni valem <Juhend />
                </Group>
              }
              leftSection={<Text size="sm" fs="italic" c="dimmed">f(x)=</Text>}
              leftSectionWidth={50}
              placeholder="nt x^3 - 3x"
              autoFocus
              autoComplete="off"
              spellCheck={false}
              styles={{ input: { fontFamily: 'var(--mantine-font-family-monospace)' } }}
              {...vorm.getInputProps('valem')}
              error={vorm.errors.valem ?? serveriViga}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 6, md: 2 }}>
            <NumberInput label="Vahemiku algus" decimalScale={4} {...vorm.getInputProps('vahemikAlgus')} />
          </Grid.Col>
          <Grid.Col span={{ base: 6, md: 2 }}>
            <NumberInput label="Vahemiku lõpp" decimalScale={4} {...vorm.getInputProps('vahemikLopp')} />
          </Grid.Col>
          <Grid.Col span={{ base: 12, md: 3 }} pt={{ base: 'xs', md: 33 }}>
            <Button type="submit" fullWidth loading={salvestab} leftSection={<IconDeviceFloppy size={18} />}>
              {nupuTekst}
            </Button>
          </Grid.Col>
        </Grid>

        <Group gap={6} mt="sm">
          <Text size="xs" c="dimmed">Näited:</Text>
          {NAITED.map((n) => (
            <Button
              key={n.valem}
              size="compact-xs"
              variant="light"
              color="gray"
              onClick={() => vorm.setValues({ valem: n.valem, vahemikAlgus: n.algus, vahemikLopp: n.lopp })}
            >
              <Valem latex={n.latex} />
            </Button>
          ))}
        </Group>
      </Card>

      {analuus.isError && !serveriViga && (
        <Alert color="red" icon={<IconAlertTriangle size={18} />}>{analuus.error.message}</Alert>
      )}

      {eelvaateAndmed && (
        <Grid gap="md">
          <Grid.Col span={{ base: 12, lg: 5 }}>
            <OmadusteTabel
              omadused={eelvaateAndmed}
              pealkiri="Vastused (veel salvestamata)"
              koikDetailid={koikDetailid}
              onKoikDetailid={setKoikDetailid}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, lg: 7 }}>
            <GraafikuPaneel
              valem={eelvaateAndmed.valem}
              algus={eelvaateAndmed.vahemikAlgus}
              lopp={eelvaateAndmed.vahemikLopp}
              koikDetailid={koikDetailid}
            />
          </Grid.Col>
        </Grid>
      )}
    </Stack>
  )
}
