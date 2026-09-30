import { Anchor, Center, Stack, Text, Title } from '@mantine/core'
import { Link } from 'react-router'

export function EiLeitud() {
  return (
    <Center h={300}>
      <Stack align="center" gap={4}>
        <Title order={3}>Lehte ei leitud</Title>
        <Text c="dimmed">
          <Anchor component={Link} to="/">Tagasi avalehele</Anchor>
        </Text>
      </Stack>
    </Center>
  )
}
