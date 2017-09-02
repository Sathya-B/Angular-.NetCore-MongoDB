import { Pipe, PipeTransform } from "@angular/core";
 
@Pipe({
    name: "filter",
    pure: false
})
export class FilterPipe implements PipeTransform {
 
    transform(items: Array<any>, conditions: string): Array<any> {
              var newValue = [];

        for (let i = 0; i < items.length; i++) {
            let keyVal = FilterPipe.deepFind(items[i], conditions);
            let index = newValue.findIndex( myObj => myObj[conditions] == keyVal);
            if (index >= 0) {
                newValue[index].variants.push(items[i]);
            } else {
                var topofgroup = {[conditions]: keyVal, topItem: items[i], variants: [items[i]]};
                newValue.push(topofgroup);
             //   newValue.push({design: keyVal, Price: items[i].product_Price, ImgUrl: items[i].minioObject_Url, variants: [items[i]]});
            }
        }
        console.log(newValue);
        return newValue;

    }

    static deepFind(obj, path) {

        var paths = path.toString().split(/[\.\[\]]/);
        var current = obj;

        for (let i = 0; i < paths.length; ++i) {
            if (paths[i] !== "") {
                if (current[paths[i]] == undefined) {
                    return undefined;
                } else {
                    current = current[paths[i]];
                }
            }
        }
        return current;
    }
}