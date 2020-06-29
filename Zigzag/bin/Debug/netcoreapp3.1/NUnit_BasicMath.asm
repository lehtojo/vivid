section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_basic_math
export function_basic_math
function_basic_math:
mov r9, rcx
imul r9, r8
add r9, rcx
add r9, r8
imul rdx, rcx
add r8, 1
imul rdx, r8
imul rdx, 100
add r9, rdx
mov rax, r9
ret

global function_addition
export function_addition
function_addition:
add rcx, rdx
mov rax, rcx
ret

global function_subtraction
export function_subtraction
function_subtraction:
sub rcx, rdx
mov rax, rcx
ret

global function_multiplication
export function_multiplication
function_multiplication:
imul rcx, rdx
mov rax, rcx
ret

global function_division
export function_division
function_division:
mov rax, rcx
mov r8, rdx
xor rdx, rdx
idiv r8
ret

global function_addition_with_constant
export function_addition_with_constant
function_addition_with_constant:
mov rax, 10
add rax, rcx
add rax, 10
ret

global function_subtraction_with_constant
export function_subtraction_with_constant
function_subtraction_with_constant:
mov rax, -10
add rax, rcx
sub rax, 10
ret

global function_multiplication_with_constant
export function_multiplication_with_constant
function_multiplication_with_constant:
mov rax, 10
imul rax, rcx
imul rax, 10
ret

global function_division_with_constant
export function_division_with_constant
function_division_with_constant:
mov rax, 100
xor rdx, rdx
idiv rcx
xor rdx, rdx
mov rcx, 10
idiv rcx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call function_addition
xor rcx, rcx
xor rdx, rdx
call function_subtraction
xor rcx, rcx
xor rdx, rdx
call function_multiplication
mov rcx, 1
mov rdx, 1
call function_division
xor rcx, rcx
call function_addition_with_constant
xor rcx, rcx
call function_subtraction_with_constant
xor rcx, rcx
call function_multiplication_with_constant
xor rcx, rcx
call function_division_with_constant
mov rcx, 1
mov rdx, 2
mov r8, 3
call function_basic_math
ret

section .data