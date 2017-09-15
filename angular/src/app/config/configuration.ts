import { OpaqueToken } from '@angular/core';

export const serverUrl = "http://localhost:5000/api/";
export const authServer = "http://localhost:5001/api/auth";

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
