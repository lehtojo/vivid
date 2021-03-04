import pow(a: decimal, b: decimal): decimal

import sqrt(a: large): decimal
import sqrt(a: decimal): decimal

import sin(a: decimal): decimal
import cos(a: decimal): decimal
import tan(a: decimal): decimal

export min(a, b) {
	if a < b => a
	else => b
}

export max(a, b) {
	if a > b => a
	else => b
}

export ceil(a: decimal) => (a + 0.5) as large
export floor(a: decimal) => a as large

export sign(a: decimal) {
	if a > 0 => 1
	else a < 0 => -1
	else => 0 
}

export cbrt(a: large) => pow(a, 1.0 / 3.0)
export cbrt(a: decimal) => pow(a, 1.0 / 3.0)

Random {
	static:
	a: large
	b: large
	c: large
	n: large
}

###
random() {
	x = Random.a + Random.b + Random.c + Random.n++
	Random.a = Random.b ¤ (Random.b |> 12)
	Random.b = Random.c + (Random.c <| 3)
	Random.c = ((Random.c <| 25) | (Random.c |> 39)) + x # 64 - 25 = 39
	=> x
}
###

export random() {
	b = Random.b
	c = Random.c
	x = Random.a + b + c + Random.n++
	Random.a = b ¤ (b |> 12)
	Random.b = c + (c <| 3)
	Random.c = ((c <| 25) | (c |> 39)) + x # 64 - 25 = 39
	=> x
}

export set_random_seed(seed: large) {
	Random.a = seed
	Random.b = seed
	Random.c = seed
	Random.n = 1

	loop (i = 0, i < 12, i++) {
		random()
	}
}