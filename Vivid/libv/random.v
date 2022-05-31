namespace internal.random {
	a: large
	b: large
	c: large
	n: large
}

# Summary: Return a random integer number
export random() {
	b = internal.random.b
	c = internal.random.c
	x = internal.random.a + b + c + internal.random.n++
	internal.random.a = b ¤ (b |> 12)
	internal.random.b = c + (c <| 3)
	internal.random.c = ((c <| 25) | (c |> 39)) + x # 64 - 25 = 39
	=> x
}

# Summary: Returns a random integer number between the specified range where a is the minimum and b the maximum.
export random(a, b) {
	=> a + (random() as u64) % (b - a)
}

# Summary: Returns a random integer number between the specified range where zero is the minimum and a the maximum.
export random(a) {
	=> (random() as u64) % a
}

export set_random_seed(seed: large) {
	internal.random.a = seed
	internal.random.b = seed
	internal.random.c = seed
	internal.random.n = 1

	loop (i = 0, i < 12, i++) {
		random()
	}
}