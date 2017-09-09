import { OpaqueToken } from '@angular/core';

export const serverUrl = "http://13.126.223.92/api/";

export class Configuration {
    apiKey: string;
    username: string;
    password: string;
    accessToken: string | (() => string);
}
export const BASE_PATH = new OpaqueToken('basePath');
export const COLLECTION_FORMATS = {
    'csv': ',',
    'tsv': '   ',
    'ssv': ' ',
    'pipes': '|'
};
