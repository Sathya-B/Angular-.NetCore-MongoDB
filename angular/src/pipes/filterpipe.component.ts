import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'filter',
    pure: true
})
export class FilterPipe implements PipeTransform {

public transform(items: any[], conditions: string): any[] {
        let newValue = [];
        for (let i = 0; i < items.length; i++) {
            let keyVal = FilterPipe.deepFind(items[i], conditions);
            let index = newValue.findIndex((myObj) => myObj[conditions] === keyVal);
            if (index >= 0) {
                newValue[index].variants.push(items[i]);
            } else {
                let topofgroup = { [conditions]: keyVal, topItem: items[i], variants: [items[i]] };
                newValue.push(topofgroup);
            }
        }
        console.log(newValue);
        return newValue;
    }
private static deepFind(obj, path) {
        let paths = path.toString().split(/[\.\[\]]/);
        let current = obj;
        for (let i = 0; i < paths.length; ++i) {
            if (paths[i] !== '') {
                if (current[paths[i]] === undefined) {
                    return undefined;
                } else {
                    current = current[paths[i]];
                }
            }
        }
        return current;
    }
}
