[![Latest Release](https://img.shields.io/badge/version-1.0.0-brightgreen.svg)](../../../../Ash.SRUtils/releases) [![Build Status](https://travis-ci.com/MillenniumWarAigis/Ash.SRUtils.svg?branch=master)](https://travis-ci.com/MillenniumWarAigis/Ash.SRUtils) ![Console App Output](https://img.shields.io/badge/output-console_app-green.svg) ![.NET Framework](https://img.shields.io/badge/%2ENET_framework-4%2E5%2E2-green.svg) ![C# Language](https://img.shields.io/badge/language-C%23-yellow.svg) [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

# Senshis' Revenge Resource File Decrypter and Encrypter Tool

## Description

This command line program can encrypt and decrypt files for the game `Senshis' Revenge`.

## Resource Files


| Files              | No Encryption      | Xor Encrypted      | CXor Encrypted     | Output                                                 |
|--------------------|--------------------|--------------------|--------------------|--------------------------------------------------------|
| N/A                |                    | :heavy_check_mark: |                    | `JPG`;`PNG`                                            |
| MP3                | :heavy_check_mark: |                    |                    | `MP3`                                                  |
| SWF                |                    |                    | :heavy_check_mark: | `SWF`                                                  |
| JPG                |                    | :heavy_check_mark: |                    | `JPG` large opaque image                               |
| PNG                |                    | :heavy_check_mark: |                    | `PNG` small transparent image                          |
| DAT                |                    | :heavy_check_mark: |                    | `ZIP` archive of `JSON` files                          |
| ANM                |                    | :heavy_check_mark: |                    | `ZIP` archive of a `PNG` spritesheet and `JSON` file   |
| MIP                |                    | :heavy_check_mark: |                    | *probably* an archive of `PNG` mip textures            |


## Usage

Drop files or directories onto the program window or icon and it will encrypt or decrypt the files to the designated output folder (by default: `out`).


## Known Issues

- Relative link operands, such as `./` and `../`, are likely not supported.


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details
