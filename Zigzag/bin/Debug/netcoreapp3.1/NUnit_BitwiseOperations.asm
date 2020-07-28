section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_bitwise_and
export function_bitwise_and
function_bitwise_and:
and rcx, rdx
mov rax, rcx
ret

global function_bitwise_xor
export function_bitwise_xor
function_bitwise_xor:
xor rcx, rdx
mov rax, rcx
ret

global function_bitwise_or
export function_bitwise_or
function_bitwise_or:
or rcx, rdx
mov rax, rcx
ret

global function_synthetic_and
export function_synthetic_and
function_synthetic_and:
mov rax, rcx
xor rax, rdx
not rax
or rcx, rdx
not rcx
xor rax, rcx
ret

global function_synthetic_xor
export function_synthetic_xor
function_synthetic_xor:
mov rax, rcx
or rax, rdx
and rcx, rdx
not rcx
and rax, rcx
ret

global function_synthetic_or
export function_synthetic_or
function_synthetic_or:
mov rax, rcx
xor rax, rdx
and rcx, rdx
xor rax, rcx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call function_bitwise_and
xor rcx, rcx
xor rdx, rdx
call function_bitwise_xor
xor rcx, rcx
xor rdx, rdx
call function_bitwise_or
xor rcx, rcx
xor rdx, rdx
call function_synthetic_and
xor rcx, rcx
xor rdx, rdx
call function_synthetic_xor
xor rcx, rcx
xor rdx, rdx
call function_synthetic_or
ret

section .data