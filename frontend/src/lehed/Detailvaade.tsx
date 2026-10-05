import { Alert, Button, Grid, Group, Loader, Stack, Text, Title } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconAlertTriangle, IconPencil, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { useFunktsioon, useKustuta } from '../api/paringud'
import { GraafikuPaneel } from '../komponendid/GraafikuPaneel'
import { OmadusteTabel } from '../komponendid/OmadusteTabel'
import { Valem } from '../komponendid/Valem'
import { kuupaev } from './vorming'

export function Detailvaade() {
  const id = Number(useParams().id)
  const kustuta = useKustuta()
  // kustutamise ajal ja järel ei laadita kirjet uuesti (see annaks 404)
  const { data: f, isLoading, error } = useFunktsioon(id, kustuta.isIdle || kustuta.isError)
  const navigeeri = useNavigate()
  const [koikDetailid, setKoikDetailid] = useState(false)

  if (isLoading) return <Loader m="xl" />
  if (error || !f)
    return (
      <Alert color="red" icon={<IconAlertTriangle size={18} />} title="Funktsiooni ei leitud">
        {error?.message}
      </Alert>
    )

  const kinnitaKustutamine = () =>
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
      onConfirm: () => kustuta.mutate(f.id, { onSuccess: () => navigeeri('/') }),
    })

  return (
    <Stack gap="md">
      <Group justify="space-between" align="flex-start" wrap="wrap">
        <div>
          <Title order={2} fw={500}>
            <Valem latex={f.valemLatex ? `f(x) = ${f.valemLatex}` : null} varuTekst={`f(x) = ${f.valem}`} />
          </Title>
          <Text size="xs" c="dimmed" mt={4}>
            Loodud {kuupaev(f.luodudAeg)}
            {f.muudetudAeg && ` · muudetud ${kuupaev(f.muudetudAeg)}`}
          </Text>
        </div>
        <Group gap="xs">
          <Button component={Link} to={`/funktsioon/${f.id}/muuda`} variant="default" leftSection={<IconPencil size={16} />}>
            Muuda
          </Button>
          <Button color="red" variant="light" leftSection={<IconTrash size={16} />} onClick={kinnitaKustutamine} loading={kustuta.isPending}>
            Kustuta
          </Button>
        </Group>
      </Group>

      <Grid gap="md">
        <Grid.Col span={{ base: 12, lg: 5 }}>
          <OmadusteTabel omadused={f} koikDetailid={koikDetailid} onKoikDetailid={setKoikDetailid} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <GraafikuPaneel valem={f.valem} algus={f.vahemikAlgus} lopp={f.vahemikLopp} koikDetailid={koikDetailid} />
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
