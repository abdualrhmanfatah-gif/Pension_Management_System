import assert from 'node:assert/strict';
import { test } from 'node:test';

import { formatYemenRelativeTime } from '../src/lib/date.ts';

test('relative notification time handles UTC, explicit offsets and legacy Yemen wall times equally', () => {
  const now = Date.parse('2026-10-11T10:05:00Z');
  const expected = formatYemenRelativeTime('2026-10-11T10:00:00Z', now);
  assert.equal(formatYemenRelativeTime('2026-10-11T13:00:00+03:00', now), expected);
  assert.equal(formatYemenRelativeTime('2026-10-11T13:00:00', now), expected);
  assert.equal(formatYemenRelativeTime('invalid', now), '');
});
