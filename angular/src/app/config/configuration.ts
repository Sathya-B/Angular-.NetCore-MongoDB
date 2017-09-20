import { OpaqueToken } from '@angular/core';

export const apiUrl = {
    serverUrl: '',
    authServer: '',
    tokenServer: ''
}

//***dev */
if (ENV !== 'production') {
apiUrl.serverUrl = 'http://192.168.0.118:5000/api/';
apiUrl.authServer = 'http://192.168.0.118:5001/api/auth';
apiUrl.tokenServer = 'http://192.168.0.118:5001/api/';
}
//***prod */
if (ENV === 'production') {
apiUrl.serverUrl = 'https://artwear.in:5003/api/';
apiUrl.authServer = 'https://artwear.in:5002/api/auth';
apiUrl.tokenServer = 'https://artwear.in:5002/api/';
}

export class Configuration {
    public apiKey: string;
    public username: string;
    public password: string;
    public accessToken: string | (() => string);
}
export const BASE_PATH = new OpaqueToken('basePath');
export const COLLECTION_FORMATS = {
    csv: ',',
    tsv: '   ',
    ssv: ' ',
    pipes: '|'
};
