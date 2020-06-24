section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_constant_permanence_and_array_copy
export function_constant_permanence_and_array_copy
function_constant_permanence_and_array_copy:
xor rax, rax
xor r8, r8
cmp r8, 10
jge function_constant_permanence_and_array_copy_L1
function_constant_permanence_and_array_copy_L0:
lea rax, [3+r8]
lea r9, [3+r8]
movzx r10, byte [rcx+r9]
mov byte [rdx+rax], r10b
add r8, 1
cmp r8, 10
jl function_constant_permanence_and_array_copy_L0
function_constant_permanence_and_array_copy_L1:
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call function_constant_permanence_and_array_copy
ret

section .data