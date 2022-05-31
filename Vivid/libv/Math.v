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

export abs(a) {
	if a > 0 => a
	else => -a
}

export ceil(a: decimal) {
	=> (a + 0.5) as large
}

export floor(a: decimal) {
	=> a as large
}

export sign(a: decimal) {
	if a > 0 => 1
	else a < 0 => -1
	else => 0 
}

export cbrt(a: large) {
	=> pow(a, 0.333333333333333)
}

export cbrt(a: decimal) {
	=> pow(a, 0.333333333333333)
}