export bitwise_and(a: tiny, b: tiny) {
   => a & b
}

export bitwise_xor(a: tiny, b: tiny) {
   => a ¤ b
}

export bitwise_or(a: tiny, b: tiny) {
   => a | b
}

export synthetic_and(a: tiny, b: tiny) {
   => !(a ¤ b) ¤ !(a | b)
}

export synthetic_xor(a: tiny, b: tiny) {
   => (a | b) & !(a & b)
}

export synthetic_or(a: tiny, b: tiny) {
   => (a ¤ b) ¤ (a & b)
}

export assign_bitwise_and(a: large) {
   a &= a / 2
   => a
}

export assign_bitwise_xor(a: large) {
   a ¤= 1
   => a
}

export assign_bitwise_or(a: large, b: large) {
   a |= b
   => a
}

init() {
   => 1
   bitwise_and(0i8, 0i8)
   bitwise_xor(0i8, 0i8)
   bitwise_or(0i8, 0i8)
   synthetic_and(0i8, 0i8)
   synthetic_xor(0i8, 0i8)
   synthetic_or(0i8, 0i8)
   assign_bitwise_and(0)
   assign_bitwise_xor(0)
   assign_bitwise_or(0, 0)
}