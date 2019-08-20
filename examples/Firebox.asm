section .text

get_mul_sum:
push ebp
mov ebp, esp

; ebp+28    f
; ebp+24    e
; ebp+20    d
; ebp+16    c
; ebp+12    b
; ebp+8     a
; ebp+4     return address
; ebp+0     saved ebp

; a = a * b + a * c + a * d + a * e + a * f
mov eax, [ebp+8]
mov ebx, [ebp+12]
mov ecx, eax ; Save a
mul ebx
; a * b => eax

;-_-_-_-_-_-
; c = a * b
; push a
; push b
; push c
; call _multliply
; _multliply:
;   
;   pop ecx  ;return var c
;   pop ebx  ;var b
;   pop eax  ;var a
;   mul ebx
;   mov ecx, ebx
;   
; ret

;c = a + b
;_addition:
;  pop ecx       ;var c
;  pop ebx       ;var b
;  pop eax       ;var a
;  add eax, ebx
;  mov ecx, eax
;ret 
;
; type Player {
; Level level
; Health health
; }   
; 
; type Health {
; num current
; num max
; }
;
; Player player = new Player()
; player.health.max
;

mov esi, [ebp-4]
mov esi, [esi+4]
mov eax, [esi+4]



;c = a - b
;_substrackt:
;  push ebp
;  mov ebp, esp
;  sub esp, 12
;  mov eax, [ebp+8]
;  mov ebx, [ebp+12]
;  add eax, ebx
;  mov [ebp+16], eax
;  mov esp, ebp
;  pop ebp
;ret
;-_-_-_-_-_-

mov ebx, eax ; Save a * b
mov eax, [ebp+16]
mul ecx
; a * c = eax

; a * b + [a * c]
add ebx, eax
; a * b + a * c => ebx

; a * d
mov eax, [ebp+20]
mul ecx
; a * d => eax

; a * b + a * c + [a * d]
add ebx, eax
; a * b + a * c + a * d => ebx

; a * e
mov eax, [ebp+24]
mul ecx
; a * e => eax

; a * b + a * c + a * d + [a * e]
add ebx, eax
; a * b + a * c + a * d + a * e => ebx

; a * f
mov eax, [ebp+28]
mul ecx
; a * f => eax

; a * b + a * c + a * d + a * e + a * f
add ebx, eax
; a * b + a * c + a * d + a * e + a * f => ebx

mov eax, ebx

; Exit
mov esp, ebp
pop ebp
ret

global _start
_start:

push ebp
mov ebp, esp
sub esp, 4

mov eax, dword [a]
mov ebx, dword [b]
mov ecx, dword [c]
mov edx, dword [d]
mov esi, dword [e]
mov edi, dword [f]

mov [ebp-4], eax

push edi
push esi
push edx
push ecx
push ebx
push dword [ebp-4]

mov eax, 0
mov ebx, 0
mov ecx, 0
mov edx, 0
mov esi, 0
mov edi, 0

call get_mul_sum

mov esp, ebp
pop ebp

; Exit
mov eax, 1
mov ebx, 0
int 80h

section .data
a dd 3
b dd 5
c dd 7
d dd 9
e dd 11
f dd 13