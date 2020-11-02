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
call _VN4PackIP7ProductP5PriceE4initEv_rS3_
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
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _VN7Product4initEv_rPS_
mov rcx, rdi
mov rdi, rax
call _VN6String4initEPh_rPS_
mov qword [rdi+8], rax
call _VN5Price4initEv_rPS_
mov qword [rax+8], rbp
mov rbp, [rsp+112]
mov byte [rax+16], bpl
mov rcx, rdi
mov rdx, rax
call _VN4PairIP7ProductP5PriceE4initES0_S2__rS3_
mov rcx, rbx
mov rdx, rsi
mov r8, rax
call _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E
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
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
mov rax, [rcx+8]
add rsp, 40
ret

global _V15enchant_productP4PackIP7ProductP5PriceEx
export _V15enchant_productP4PackIP7ProductP5PriceEx
_V15enchant_productP4PackIP7ProductP5PriceEx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product7enchantEv
add rsp, 40
ret

global _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
export _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product12is_enchantedEv_rx
add rsp, 40
ret

global _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
export _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
_V17get_product_priceP4PackIP7ProductP5PriceExc_rd:
push rbx
sub rsp, 48
mov rbx, r8
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+16]
mov rdx, rbx
call _VN5Price7convertEc_rd
add rsp, 48
pop rbx
ret

_V4initv_rx:
mov rax, 1
ret

_VN7Product4initEv_rPS_:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
add rsp, 40
ret

_VN7Product7enchantEv:
push rbx
sub rsp, 48
mov rbx, rcx
lea rcx, [rel _VN7Product7enchantEv_S0]
call _VN6String4initEPh_rPS_
mov rcx, rax
mov rdx, [rbx+8]
call _VN6String4plusEPS__rS0_
mov qword [rbx+8], rax
add rsp, 48
pop rbx
ret

_VN7Product12is_enchantedEv_rx:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, [rbx+8]
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

_VN5Price4initEv_rPS_:
sub rsp, 40
mov rcx, 17
call _V8allocatex_rPh
add rsp, 40
ret

_VN5Price7convertEc_rd:
movsx r8, byte [rcx+16]
cmp r8, rdx
jne _VN5Price7convertEc_rd_L0
cvtsi2sd xmm0, qword [rcx+8]
ret
_VN5Price7convertEc_rd_L0:
test rdx, rdx
jne _VN5Price7convertEc_rd_L3
cvtsi2sd xmm0, qword [rcx+8]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C0]
mulsd xmm0, xmm1
ret
jmp _VN5Price7convertEc_rd_L2
_VN5Price7convertEc_rd_L3:
cvtsi2sd xmm0, qword [rcx+8]
movsd xmm1, qword [rel _VN5Price7convertEc_rd_C1]
mulsd xmm0, xmm1
ret
_VN5Price7convertEc_rd_L2:
ret

_VN4PackIP7ProductP5PriceE4initEv_rS3_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
add rsp, 40
ret

_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L1
mov rax, [rcx+8]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L3
mov rax, [rcx+16]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L5
mov rax, [rcx+24]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L5:
xor rax, rax
ret
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0:
ret

_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1
mov qword [rcx+8], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3
mov qword [rcx+16], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
mov qword [rcx+24], r8
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0:
ret

_VN4PairIP7ProductP5PriceE4initES0_S2__rS3_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
mov qword [rax+8], rbx
mov qword [rax+16], rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN6String4initEPh_rPS_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword [rax+8], rbx
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
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov rcx, [rsi+8]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxPS_x
mov rcx, rbx
call _VN6String4initEPh_rPS_
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
mov r8, [rcx+8]
movzx rax, byte [r8+rdx]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_VN4Pair_configuration:
dq _VN4Pair_descriptor

_VN4Pair_descriptor:
dq _VN4Pair_descriptor_0
dd 8
dd 0

_VN4Pair_descriptor_0:
db 'Pair', 0

_VN4Pack_configuration:
dq _VN4Pack_descriptor

_VN4Pack_descriptor:
dq _VN4Pack_descriptor_0
dd 8
dd 0

_VN4Pack_descriptor_0:
db 'Pack', 0

_VN7Product_configuration:
dq _VN7Product_descriptor

_VN7Product_descriptor:
dq _VN7Product_descriptor_0
dd 16
dd 0

_VN7Product_descriptor_0:
db 'Product', 0

_VN5Price_configuration:
dq _VN5Price_descriptor

_VN5Price_descriptor:
dq _VN5Price_descriptor_0
dd 17
dd 0

_VN5Price_descriptor_0:
db 'Price', 0

_VN4PackIP7ProductP5PriceE_configuration:
dq _VN4PackIP7ProductP5PriceE_descriptor

_VN4PackIP7ProductP5PriceE_descriptor:
dq _VN4PackIP7ProductP5PriceE_descriptor_0
dd 32
dd 0

_VN4PackIP7ProductP5PriceE_descriptor_0:
db 'Pack<Product, Price>', 0

_VN4PairIP7ProductP5PriceE_configuration:
dq _VN4PairIP7ProductP5PriceE_descriptor

_VN4PairIP7ProductP5PriceE_descriptor:
dq _VN4PairIP7ProductP5PriceE_descriptor_0
dd 24
dd 0

_VN4PairIP7ProductP5PriceE_descriptor_0:
db 'Pair<Product, Price>', 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

align 16
_VN7Product7enchantEv_S0 db 'i', 0
align 16
_VN5Price7convertEc_rd_C0 db 154, 153, 153, 153, 153, 153, 233, 63 ; 0.8
align 16
_VN5Price7convertEc_rd_C1 db 0, 0, 0, 0, 0, 0, 244, 63 ; 1.25