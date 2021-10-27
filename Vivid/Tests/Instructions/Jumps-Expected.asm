.intel_syntax noprefix
.section .text

# ---------- Unconditional jumps ----------

# Jumping distance of zero
jmp L0
L0:

# Normal short jump
jmp L1
add rax, 1
sub rax, 1
L1:

# Normal short jump that goes backwards
jmp L0

# Normal long jump
jmp L2

mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff

L2:

# Normal long jump that goes backwards
jmp L0

# Jumping backwards inside a 'single module'
L3:
add rax, 1
sub rax, 1
jmp L3

# Register jumps
jmp rax
jmp r8

# ---------- Conditional jumps ----------

# Jumping distance of zero
jz CL0
CL0:

# Normal short jump
jz CL1
add rax, 1
sub rax, 1
CL1:

# Normal short jump that goes backwards
jz CL0

# Normal long jump
jz CL2

mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff
mov qword ptr [rax*8+r8+0x7fffffff], 0x7fffffff

CL2:

# Normal long jump that goes backwards
jz CL0

# Jumping backwards inside a 'single module'
CL3:
add rax, 1
sub rax, 1
jz CL3
