# SLASH-128 bit
A 128 bit substitution permutation network block cipher


SLASH is a 10 round substitution-permutation network with a 128 bit block size and a 256 bit internal key.

The current operating mode of this cipher is Cipher Block Chaining mode (CBC) - Though I intend to add more modes of operation in the upcoming future. 

Round structure is as shown in 'SLASH 128 bit round function.png' file. Each round conponment is unique for each round and key derived using a custom sponge construction hash
of the main key that has a slightly different string XOR'ed to it eg("SBOX_KEY" for sbox generation,"PBOX_KEY" for pbox generation,"ROUND_KEY" for round key generation).The aim
of this is to make it difficult for an attacker to derive the main key or any other round conponments from any leaked sboxes,pboxes or round keys. A diagram can be shown of the key schedule in the file "SLASH 128 bit key schedule.png"


This SPN uses a ARX based mix function for diffusion(as shown in "SLASH 32 bit mix function.png", derived from Salsa20). This mix function has an input and output of 4 bytes,
on its own this function provides very little security, so the pboxes,sboxes and round keys are needed to make this secure. Each individual step within each round can be
considered cryptographically weak, combined together and they are strong.

A general structure diagram of how all the code files fit together can be seen in the file "SLASH 128bit class diagram.png"
