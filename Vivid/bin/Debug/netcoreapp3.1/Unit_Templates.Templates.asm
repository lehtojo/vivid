.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V11offset_copyPhxS_x
.extern _V17internal_allocatex_rPh

.global _VN7Product7enchantEv
_VN7Product7enchantEv:
push rbx
sub rsp, 32
mov rbx, rcx
lea rcx, [rip+_VN7Product7enchantEv_S0]
call _VN6String4initEPh_rPS_
mov rcx, rax
mov rdx, [rbx+8]
call _VN6String4plusEPS__rS0_
mov qword ptr [rbx+8], rax
add rsp, 32
pop rbx
ret

.global _VN7Product12is_enchantedEv_rx
_VN7Product12is_enchantedEv_rx:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, [rbx+8]
xor rdx, rdx
call _VN6String3getEx_rh
movzx rax, al
cmp rax, 105
jne _VN7Product12is_enchantedEv_rx_L0
mov rax, 1
add rsp, 32
pop rbx
ret
_VN7Product12is_enchantedEv_rx_L0:
xor rax, rax
add rsp, 32
pop rbx
ret

.global _VN5Price7convertEc_rd
_VN5Price7convertEc_rd:
movsx r8, byte ptr [rcx+16]
cmp r8, rdx
jne _VN5Price7convertEc_rd_L0
cvtsi2sd xmm0, qword ptr [rcx+8]
ret
_VN5Price7convertEc_rd_L0:
test rdx, rdx
jne _VN5Price7convertEc_rd_L3
cvtsi2sd xmm0, qword ptr [rcx+8]
movsd xmm1, qword ptr [rip+_VN5Price7convertEc_rd_C0]
mulsd xmm0, xmm1
ret
jmp _VN5Price7convertEc_rd_L2
_VN5Price7convertEc_rd_L3:
cvtsi2sd xmm0, qword ptr [rcx+8]
movsd xmm1, qword ptr [rip+_VN5Price7convertEc_rd_C1]
mulsd xmm0, xmm1
ret
_VN5Price7convertEc_rd_L2:
ret

.global _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
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

.global _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1
mov qword ptr [rcx+8], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3
mov qword ptr [rcx+16], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
mov qword ptr [rcx+24], r8
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0:
ret

.global _VN7Product4initEv_rPS_
_VN7Product4initEv_rPS_:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
add rsp, 40
ret

.global _VN5Price4initEv_rPS_
_VN5Price4initEv_rPS_:
sub rsp, 40
mov rcx, 17
call _V8allocatex_rPh
add rsp, 40
ret

.global _VN4PackIP7ProductP5PriceE4initEv_rS3_
_VN4PackIP7ProductP5PriceE4initEv_rS3_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
add rsp, 40
ret

.global _VN4PairIP7ProductP5PriceE4initES0_S2__rS3_
_VN4PairIP7ProductP5PriceE4initES0_S2__rS3_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
mov qword ptr [rax+8], rbx
mov qword ptr [rax+16], rsi
add rsp, 40
pop rsi
pop rbx
ret

.global _V11create_packv_rP4PackIP7ProductP5PriceE
_V11create_packv_rP4PackIP7ProductP5PriceE:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE4initEv_rS3_
add rsp, 40
ret

.global _V11set_productP4PackIP7ProductP5PriceExPhxc
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
mov qword ptr [rdi+8], rax
call _VN5Price4initEv_rPS_
mov qword ptr [rax+8], rbp
mov rbp, [rsp+112]
mov byte ptr [rax+16], bpl
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

.global _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
mov rax, [rcx+8]
add rsp, 40
ret

.global _V15enchant_productP4PackIP7ProductP5PriceEx
_V15enchant_productP4PackIP7ProductP5PriceEx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product7enchantEv
add rsp, 40
ret

.global _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product12is_enchantedEv_rx
add rsp, 40
ret

.global _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
_V17get_product_priceP4PackIP7ProductP5PriceExc_rd:
push rbx
sub rsp, 32
mov rbx, r8
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+16]
mov rdx, rbx
call _VN5Price7convertEc_rd
add rsp, 32
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
mov rax, 1
ret

.section .data

_VN4Pair_configuration:
.quad _VN4Pair_descriptor

_VN4Pair_descriptor:
.quad _VN4Pair_descriptor_0
.long 8
.long 0

_VN4Pair_descriptor_0:
.ascii "Pair"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4Pack_configuration:
.quad _VN4Pack_descriptor

_VN4Pack_descriptor:
.quad _VN4Pack_descriptor_0
.long 8
.long 0

_VN4Pack_descriptor_0:
.ascii "Pack"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Product_configuration:
.quad _VN7Product_descriptor

_VN7Product_descriptor:
.quad _VN7Product_descriptor_0
.long 16
.long 0

_VN7Product_descriptor_0:
.ascii "Product"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5Price_configuration:
.quad _VN5Price_descriptor

_VN5Price_descriptor:
.quad _VN5Price_descriptor_0
.long 17
.long 0

_VN5Price_descriptor_0:
.ascii "Price"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4PackIP7ProductP5PriceE_configuration:
.quad _VN4PackIP7ProductP5PriceE_descriptor

_VN4PackIP7ProductP5PriceE_descriptor:
.quad _VN4PackIP7ProductP5PriceE_descriptor_0
.long 32
.long 0

_VN4PackIP7ProductP5PriceE_descriptor_0:
.ascii "Pack<Product, Price>"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4PairIP7ProductP5PriceE_configuration:
.quad _VN4PairIP7ProductP5PriceE_descriptor

_VN4PairIP7ProductP5PriceE_descriptor:
.quad _VN4PairIP7ProductP5PriceE_descriptor_0
.long 24
.long 0

_VN4PairIP7ProductP5PriceE_descriptor_0:
.ascii "Pair<Product, Price>"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_VN7Product7enchantEv_S0:
.ascii "i"
.byte 0

.balign 16
_VN5Price7convertEc_rd_C0:
.byte 154, 153, 153, 153, 153, 153, 233, 63 # 0.8
.balign 16
_VN5Price7convertEc_rd_C1:
.byte 0, 0, 0, 0, 0, 0, 244, 63 # 1.25

