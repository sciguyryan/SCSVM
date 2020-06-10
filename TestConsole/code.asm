.section data
str db 'Hello, world!',$0xA
strLen equ $-str

.section text
mov str, R8
mov BYTE &R8, R7
mov strLen, R6
push $0xAAA  ; Should remain in place once the stack is restored
push $0xC    ; TESTER Argument 3
push $0xB    ; TESTER Argument 2
push $0xA    ; TESTER Argument 1
push $3      ; The number of arguments for the subroutine
call !TESTER
mov $0x123, R1
hlt

TESTER:
mov $0x34, &FP, R3
mov $0x30, &FP, R2
mov $0x2C, &FP, R1
add R1, R2
add R3, AC
push $0xCC    ; TESTER2 Argument 3
push $0xBB    ; TESTER2 Argument 2
push $0xAA    ; TESTER2 Argument 1
push $3       ; The number of arguments for the subroutine
call !TESTER2
ret

TESTER2:
mov $0x34, &FP, R3
mov $0x30, &FP, R2
mov $0x2C, &FP, R1
add R1, R2
add R3, AC
ret