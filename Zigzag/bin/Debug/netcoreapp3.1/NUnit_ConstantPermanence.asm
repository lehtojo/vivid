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
xor rax, rax
cmp rax, 10
jge function_constant_permanence_and_array_copy_L1
function_constant_permanence_and_array_copy_L0:
lea r8, [3+rax]
lea r9, [3+rax]
movzx r10, byte [rcx+r9]
mov byte [rdx+r8], r10b
add rax, 1
cmp rax, 10
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