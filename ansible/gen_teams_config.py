tmpl = "  {{name => '{}', network => '10.118.{}.0/24', host => '10.118.{}.0', team_id => '{}', token => '{}', logo => '/data/logos/{}'}},"

lines = []

import random
x = '0123456789abcdef'

names = [
'Команда Лучкиных Вячеславов',
'N0N@me13',
'C4T BuT S4D',
'Импактный SUSlo.PAS',
'Red Cadets',
'Punk Souls',
'SPRUSH',
'♿️🅵🅰️🅺🅰️🅿️🅿️🅰️♿️',
'SFT0',
'Lunary',
]



for i in range(101, 111):
    t = ''.join(random.choice(x) for i in range(20))
    s = tmpl.format(names[i-101], i, i, i, t, i)
    lines.append(s)

print("\n".join(lines))
