//
//  InfinarioBridge.m
//  InfinarioSDK
//
//  Created by Roland Rogansky on 10/07/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//
#import "Infinario.h"

NSString* createNSString(const char* value){
    if (value){
        return [NSString stringWithUTF8String: value];
    } else {
        return nil;
    }
}

NSNumber* createNSNumber(const char* value){
    if (value){
        NSNumberFormatter *f = [[NSNumberFormatter alloc] init];
        f.numberStyle = NSNumberFormatterDecimalStyle;
        return [f numberFromString: createNSString(value)];
    } else {
        return nil;
    }
}

NSDictionary* createNSDictionary(const char* value){
    if (value){
        NSError *error;
        NSString *dictString=[NSString stringWithUTF8String: value];
        NSData *jsonData = [dictString dataUsingEncoding:NSUTF8StringEncoding];
        return [NSJSONSerialization JSONObjectWithData:jsonData
                                               options:NSJSONReadingMutableContainers
                                                 error:&error];
    } else {
        return nil;
    }
}

extern "C"
{
    Infinario *infinario;
    
    void _shareInstanceWithToken(const char* companyToken, const char* target){
        infinario = [Infinario sharedInstanceWithToken: createNSString(companyToken) andWithTarget: createNSString(target)];
    }
    
    void _track(const char* type, const char* properties, const char* timestamp){
        if (infinario){
            [infinario track:createNSString(type) withProperties:createNSDictionary(properties) withTimestamp: createNSNumber(timestamp)];
        }
    }
    
    void _trackVirtualPayment(const char* currency, const char* amount, const char* itemName, const char* itemType){
        [infinario trackVirtualPayment:createNSString(currency) withAmount: createNSNumber(amount) withItemName: createNSString(itemName) withItemType: createNSString(itemType)];
    }
    
    void _identifyWithCustomer(const char* customerId, const char* properties){
        [infinario identifyWithCustomer: createNSString(customerId) andUpdate: createNSDictionary: properties];
    }
    
    void _update(const char* properties){
        [infinario update:createNSDictionary(properties)];
    }
    
    void _registrationPushNotification(){
        [infinario registerPushNotifications];
    }
    
    void _addPushNotificationToken(const char* deviceToken){
        [infinario addPushNotificationsToken: createNSString(deviceToken)];
    }
    
    void _enableAutomaticFlushing(){
        [infinario enableAutomaticFlushing];
    }
    
    void _disableAutomaticFlushing(){
        [infinario disableAutomaticFlushing];
    }
    
    void _flush(){
        [infinario flush];
    }
}