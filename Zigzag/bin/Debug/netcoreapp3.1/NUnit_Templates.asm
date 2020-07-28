section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_create_pack
export function_create_pack
function_create_pack:
sub rsp, 40
mov rcx, 24
call allocate
add rsp, 40
ret

global function_set_product
export function_set_product
function_set_product:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rax, rcx
mov rcx, 8
mov rbx, rax
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call allocate
mov rcx, rdi
mov rdi, rax
call type_string_constructor
mov qword [rdi], rax
mov rcx, 9
call allocate
mov qword [rax], rbp
movsx rbp, byte [rsp+112]
mov byte [rax+8], bpl
mov rcx, rdi
mov rdx, rax
call type_pair_product_price_constructor
mov rcx, rbx
mov rdx, rsi
mov r8, rax
call type_pack_product_price_function_set
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

global function_get_product_name
export function_get_product_name
function_get_product_name:
sub rsp, 40
call type_pack_product_price_function_get
mov rcx, [rax]
mov rax, [rcx]
add rsp, 40
ret

global function_enchant_product
export function_enchant_product
function_enchant_product:
sub rsp, 40
call type_pack_product_price_function_get
mov rcx, [rax]
call type_product_function_enchant
add rsp, 40
ret

global function_is_product_enchanted
export function_is_product_enchanted
function_is_product_enchanted:
sub rsp, 40
call type_pack_product_price_function_get
mov rcx, [rax]
call type_product_function_is_enchanted
add rsp, 40
ret

global function_get_product_price
export function_get_product_price
function_get_product_price:
push rbx
sub rsp, 48
mov rbx, r8
call type_pack_product_price_function_get
mov rcx, [rax+8]
mov rdx, rbx
call type_price_function_convert
add rsp, 48
pop rbx
ret

function_run:
push rbx
sub rsp, 48
mov rax, 1
add rsp, 48
pop rbx
ret
call function_create_pack
mov rcx, rax
xor rdx, rdx
xor r8, r8
xor r9, r9
mov byte [rsp+32], 0
mov rbx, rax
call function_set_product
mov rcx, rbx
xor rdx, rdx
call function_get_product_name
mov rcx, rbx
xor rdx, rdx
call function_enchant_product
mov rcx, rbx
xor rdx, rdx
call function_is_product_enchanted
mov rcx, rbx
xor rdx, rdx
xor r8, r8
call function_get_product_price
pop rbx
ret

type_product_function_enchant:
push rbx
sub rsp, 48
mov rax, rcx
lea rcx, [rel type_product_function_enchant_S0]
mov rbx, rax
call type_string_constructor
mov rcx, rax
mov rdx, [rbx]
call type_string_function_plus
mov qword [rbx], rax
add rsp, 48
pop rbx
ret

type_product_function_is_enchanted:
push rbx
sub rsp, 48
mov rdx, rcx
mov rcx, [rdx]
mov r8, rdx
xor rdx, rdx
mov rbx, r8
call type_string_function_get
cmp al, 105
mov rcx, rbx
jne type_product_function_is_enchanted_L0
mov rax, 1
add rsp, 48
pop rbx
ret
type_product_function_is_enchanted_L0:
xor rax, rax
add rsp, 48
pop rbx
ret

type_price_function_convert:
movsx r8, byte [rcx+8]
cmp r8, rdx
jne type_price_function_convert_L0
cvtsi2sd xmm0, qword [rcx]
ret
type_price_function_convert_L0:
test rdx, rdx
jne type_price_function_convert_L3
cvtsi2sd xmm0, qword [rcx]
movsd xmm1, qword [rel type_price_function_convert_C0]
mulsd xmm0, xmm1
ret
jmp type_price_function_convert_L2
type_price_function_convert_L3:
cvtsi2sd xmm0, qword [rcx]
movsd xmm1, qword [rel type_price_function_convert_C1]
mulsd xmm0, xmm1
ret
type_price_function_convert_L2:
ret

type_pack_product_price_function_get:
test rdx, rdx
jne type_pack_product_price_function_get_L1
mov rax, [rcx]
ret
jmp type_pack_product_price_function_get_L0
type_pack_product_price_function_get_L1:
cmp rdx, 1
jne type_pack_product_price_function_get_L3
mov rax, [rcx+8]
ret
jmp type_pack_product_price_function_get_L0
type_pack_product_price_function_get_L3:
cmp rdx, 2
jne type_pack_product_price_function_get_L5
mov rax, [rcx+16]
ret
jmp type_pack_product_price_function_get_L0
type_pack_product_price_function_get_L5:
xor rax, rax
ret
type_pack_product_price_function_get_L0:
ret

type_pack_product_price_function_set:
test rdx, rdx
jne type_pack_product_price_function_set_L1
mov qword [rcx], r8
jmp type_pack_product_price_function_set_L0
type_pack_product_price_function_set_L1:
cmp rdx, 1
jne type_pack_product_price_function_set_L3
mov qword [rcx+8], r8
jmp type_pack_product_price_function_set_L0
type_pack_product_price_function_set_L3:
cmp rdx, 2
jne type_pack_product_price_function_set_L0
mov qword [rcx+16], r8
type_pack_product_price_function_set_L0:
ret

type_pair_product_price_constructor:
push rbx
push rsi
sub rsp, 40
mov r8, rcx
mov rcx, 16
mov rbx, rdx
mov rsi, r8
call allocate
mov qword [rax], rsi
mov qword [rax+8], rbx
add rsp, 40
pop rsi
pop rbx
ret

type_string_constructor:
push rbx
sub rsp, 48
mov rdx, rcx
mov rcx, 8
mov rbx, rdx
call allocate
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

type_string_function_combine:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call type_string_function_length
mov rcx, rsi
mov rdi, rax
call type_string_function_length
add rax, 1
lea rcx, [rdi+rax]
mov rbp, rax
call allocate
mov rcx, [rbx]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call copy
mov rcx, [rsi]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call offset_copy
mov rcx, rbx
call type_string_constructor
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

type_string_function_plus:
sub rsp, 40
call type_string_function_combine
add rsp, 40
ret

type_string_function_get:
mov r8, [rcx]
movzx rax, byte [r8+rdx]
ret

type_string_function_length:
xor rax, rax
mov r8, [rcx]
movzx rdx, byte [r8+rax]
test rdx, rdx
je type_string_function_length_L1
type_string_function_length_L0:
add rax, 1
mov r8, [rcx]
movzx rdx, byte [r8+rax]
test rdx, rdx
jne type_string_function_length_L0
type_string_function_length_L1:
ret

section .data

type_product_function_enchant_S0 db 'i', 0
type_price_function_convert_C0 dq 0.8
type_price_function_convert_C1 dq 1.25