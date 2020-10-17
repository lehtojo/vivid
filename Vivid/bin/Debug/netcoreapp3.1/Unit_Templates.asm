section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x

global _V11create_packv_rP4PackIP7ProductP5PriceE
export _V11create_packv_rP4PackIP7ProductP5PriceE
_V11create_packv_rP4PackIP7ProductP5PriceE:
sub rsp, 40
mov rcx, 24
call _V8allocatex_rPh
add rsp, 40
ret

global _V11set_productP4PackIP7ProductP5PriceExPhxc
export _V11set_productP4PackIP7ProductP5PriceExPhxc
_V11set_productP4PackIP7ProductP5PriceExPhxc:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, 8
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
mov rcx, rdi
mov rdi, rax
call _VN6String4initEPh_rS0_
mov qword [rdi], rax
mov rcx, 9
call _V8allocatex_rPh
mov qword [rax], rbp
mov rbp, [rsp+112]
mov byte [rax+8], bpl
mov rcx, rdi
mov rdx, rax
call _VN4PairIP7ProductP5PriceE4initES1_S3__rPh
mov rcx, rbx
mov rdx, rsi
mov r8, rax
call _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

global _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
export _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rcx, [rax]
mov rax, [rcx]
add rsp, 40
ret

global _V15enchant_productP4PackIP7ProductP5PriceEx
export _V15enchant_productP4PackIP7ProductP5PriceEx
_V15enchant_productP4PackIP7ProductP5PriceEx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rcx, [rax]
call _VN7Product7enchantEv
add rsp, 40
ret

global _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
export _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rcx, [rax]
call _VN7Product12is_enchantedEv_rx
add rsp, 40
ret

global _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
export _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
_V17get_product_priceP4PackIP7ProductP5PriceExc_rd:
push rbx
sub rsp, 48
mov rbx, r8
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E
mov rcx, [rax+8]
mov rdx, rbx
call _VN5Price7convertEc_rd
add rsp, 48
pop rbx
ret

_V4initv_rx:
mov rax, 1
ret

_VN7Product7enchantEv:
push rbx
sub rsp, 48
mov rbx, rcx
lea rcx, [rel _VN7Product7enchantEv_S0]
call _VN6String4initEPh_rS0_
mov rcx, rax
mov rdx, [rbx]
call _VN6String4plusEPS__rS0_
mov qword [rbx], rax
add rsp, 48
pop rbx
ret

_VN7Product12is_enchantedEv_rx:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, [rbx]
xor rdx, rdx
call _VN6String3getEx_rh
movzx rax, al
cmp rax, 105
mov rcx, rbx
jne _VN7Product12is_enchantedEv_rx_L0
mov rax, 1
add rsp, 48
pop rbx
ret
_VN7Product12is_enchantedEv_rx_L0:
xor rax, rax
add rsp, 48
pop rbx
ret

_VN5Price7convertEc_rd:
movsx r8, byte [rcx+8]
cmp r8, rdx
jne _VN5Price7convertEc_rd_L0
cvtsi2sd xmm0, qword [rcx]
ret
_VN5Price7convertEc_rd_L0:
test rdx, rdx
jne _VN5Price7convertEc_rd_L3
cvtsi2sd xmm0, qword [rcx]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C0]
mulsd xmm0, xmm1
ret
jmp _VN5Price7convertEc_rd_L2
_VN5Price7convertEc_rd_L3:
cvtsi2sd xmm0, qword [rcx]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C1]
mulsd xmm0, xmm1
ret
_VN5Price7convertEc_rd_L2:
ret

_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L1
mov rax, [rcx]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L3
mov rax, [rcx+8]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L5
mov rax, [rcx+16]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L5:
xor rax, rax
ret
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS1_S3_E_L0:
ret

_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L1
mov qword [rcx], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L3
mov qword [rcx+8], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0
mov qword [rcx+16], r8
_VN4PackIP7ProductP5PriceE3setExP4PairIS1_S3_E_L0:
ret

_VN4PairIP7ProductP5PriceE4initES1_S3__rPh:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 16
mov rsi, rdx
call _V8allocatex_rPh
mov qword [rax], rbx
mov qword [rax+8], rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 8
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

_VN6String7combineEPS__rS0_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _VN6String6lengthEv_rx
add rax, 1
lea rcx, [rdi+rax]
mov rbp, rax
call _V8allocatex_rPh
mov rcx, [rbx]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov rcx, [rsi]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxPS_x
mov rcx, rbx
call _VN6String4initEPh_rS0_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String4plusEPS__rS0_:
sub rsp, 40
call _VN6String7combineEPS__rS0_
add rsp, 40
ret

_VN6String3getEx_rh:
mov r8, [rcx]
movzx rax, byte [r8+rdx]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov r8, [rcx]
movzx rdx, byte [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx]
movzx rdx, byte [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

align 16
_VN7Product7enchantEv_S0 db 'i', 0
align 16
_VN5Price7convertEc_rd_C0 db 154, 153, 153, 153, 153, 153, 233, 63 ; 0.8
align 16
_VN5Price7convertEc_rd_C1 db 0, 0, 0, 0, 0, 0, 244, 63 ; 1.25