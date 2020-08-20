section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x

global _V11create_packv_rP4PackIP7ProductP5PriceE
_V11create_packv_rP4PackIP7ProductP5PriceE:
sub rsp, 8
mov rdi, 24
call _V8allocatex_rPh
add rsp, 8
ret

global _V11set_productP4PackIP7ProductP5PriceExPhxc
_V11set_productP4PackIP7ProductP5PriceExPhxc:
push rbx
push rbp
push r12
push r13
push r14
sub rsp, 16
mov rax, rdi
mov rdi, 8
mov rbx, rax
mov rbp, rcx
mov r12, rdx
mov r13, rsi
mov r14, r8
call _V8allocatex_rPh
mov rdi, r12
mov r12, rax
call _VN6String4initEPh_rS0_
mov qword [r12], rax
mov rdi, 9
call _V8allocatex_rPh
mov qword [rax], rbp
mov byte [rax+8], r14b
mov rdi, r12
mov rsi, rax
call _VN4PairIP7ProductP5PriceE4initES1_S3__rPh
mov rdi, rbx
mov rsi, r13
mov rdx, rax
call _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E
add rsp, 16
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

global _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String:
sub rsp, 8
mov rcx, rsi
mov rsi, rcx
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rcx, [rax]
mov rax, [rcx]
add rsp, 8
ret

global _V15enchant_productP4PackIP7ProductP5PriceEx
_V15enchant_productP4PackIP7ProductP5PriceEx:
sub rsp, 8
mov rax, rsi
mov rsi, rax
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rdi, [rax]
call _VN7Product7enchantEv
add rsp, 8
ret

global _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx:
sub rsp, 8
mov rcx, rsi
mov rsi, rcx
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rdi, [rax]
call _VN7Product12is_enchantedEv_rx
add rsp, 8
ret

global _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
_V17get_product_priceP4PackIP7ProductP5PriceExc_rd:
push rbx
sub rsp, 16
mov rcx, rsi
mov rsi, rcx
mov rbx, rdx
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rdi, [rax+8]
mov rsi, rbx
call _VN5Price7convertEc_rd
add rsp, 16
pop rbx
ret

_V4initv_rx:
push rbx
sub rsp, 16
mov rax, 1
add rsp, 16
pop rbx
ret
call _V11create_packv_rP4PackIP7ProductP5PriceE
mov rdi, rax
xor rsi, rsi
xor rdx, rdx
xor rcx, rcx
xor r8, r8
mov rbx, rax
call _V11set_productP4PackIP7ProductP5PriceExPhxc
mov rdi, rbx
xor rsi, rsi
call _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
mov rdi, rbx
xor rsi, rsi
call _V15enchant_productP4PackIP7ProductP5PriceEx
mov rdi, rbx
xor rsi, rsi
call _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
mov rdi, rbx
xor rsi, rsi
xor rdx, rdx
call _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
pop rbx
ret

_VN7Product7enchantEv:
push rbx
sub rsp, 16
mov rax, rdi
lea rdi, [rel _VN7Product7enchantEv_S0]
mov rbx, rax
call _VN6String4initEPh_rS0_
mov rdi, rax
mov rsi, [rbx]
call _VN6String4plusEPS__rS0_
mov qword [rbx], rax
add rsp, 16
pop rbx
ret

_VN7Product12is_enchantedEv_rx:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, [rcx]
xor rsi, rsi
mov rbx, rcx
call _VN6String3getEx_rh
movzx rax, al
cmp rax, 105
mov rdi, rbx
jne _VN7Product12is_enchantedEv_rx_L0
mov rax, 1
add rsp, 16
pop rbx
ret
_VN7Product12is_enchantedEv_rx_L0:
xor rax, rax
add rsp, 16
pop rbx
ret

_VN5Price7convertEc_rd:
movsx rcx, byte [rdi+8]
movsx rsi, sil
cmp rcx, rsi
jne _VN5Price7convertEc_rd_L0
cvtsi2sd xmm0, qword [rdi]
ret
_VN5Price7convertEc_rd_L0:
movsx rsi, sil
test rsi, rsi
jne _VN5Price7convertEc_rd_L3
cvtsi2sd xmm0, qword [rdi]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C0]
mulsd xmm0, xmm1
ret
jmp _VN5Price7convertEc_rd_L2
_VN5Price7convertEc_rd_L3:
cvtsi2sd xmm0, qword [rdi]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C1]
mulsd xmm0, xmm1
ret
_VN5Price7convertEc_rd_L2:
ret

_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E:
test rsi, rsi
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L1
mov rax, [rdi]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L1:
cmp rsi, 1
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L3
mov rax, [rdi+8]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L3:
cmp rsi, 2
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L5
mov rax, [rdi+16]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L5:
xor rax, rax
ret
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0:
ret

_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E:
test rsi, rsi
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L1
mov qword [rdi], rdx
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L1:
cmp rsi, 1
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L3
mov qword [rdi+8], rdx
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L3:
cmp rsi, 2
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
mov qword [rdi+16], rdx
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0:
ret

_VN4PairIP7ProductP5PriceE4initES1_S3__rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
mov rbp, rsi
call _V8allocatex_rPh
mov qword [rax], rbx
mov qword [rax+8], rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, 8
mov rbx, rcx
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 16
pop rbx
ret

_VN6String7combineEPS__rS0_:
push rbx
push rbp
push r12
push r13
sub rsp, 8
mov rbx, rsi
mov rbp, rdi
call _VN6String6lengthEv_rx
mov rdi, rbx
mov r12, rax
call _VN6String6lengthEv_rx
add rax, 1
lea rdi, [r12+rax]
mov r13, rax
call _V8allocatex_rPh
mov rdi, [rbp]
mov rsi, r12
mov rdx, rax
mov rbp, rax
call _V4copyPhxPS_
mov rdi, [rbx]
mov rsi, r13
mov rdx, rbp
mov rcx, r12
call _V11offset_copyPhxPS_x
mov rdi, rbp
call _VN6String4initEPh_rS0_
add rsp, 8
pop r13
pop r12
pop rbp
pop rbx
ret

_VN6String4plusEPS__rS0_:
sub rsp, 8
call _VN6String7combineEPS__rS0_
add rsp, 8
ret

_VN6String3getEx_rh:
mov rcx, [rdi]
movzx rax, byte [rcx+rsi]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_VN7Product7enchantEv_S0 db 'i', 0
_VN5Price7convertEc_rd_C0 dq 0.8
_VN5Price7convertEc_rd_C1 dq 1.25